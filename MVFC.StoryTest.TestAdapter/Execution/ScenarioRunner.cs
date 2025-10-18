namespace MVFC.StoryTest.TestAdapter.Execution;

/// <summary>
/// Responsável por executar cenários de teste BDD, gerenciando o ciclo de vida, execução de steps e hooks.
/// </summary>
public sealed class ScenarioRunner {
    /// <summary>
    /// Executa um cenário BDD, processando todos os steps e hooks associados.
    /// </summary>
    /// <param name="scenarioType">Tipo da classe do cenário.</param>
    /// <param name="steps">Array de steps a serem executados.</param>
    /// <param name="messageCallback">Callback para mensagens de log.</param>
    /// <returns>Resultado da execução do cenário.</returns>
    public static ExecutionResult RunScenario(Type scenarioType, StepInfo[] steps, Action<string> messageCallback = null!) {
        var result = new ExecutionResult {
            StartTime = DateTimeOffset.Now,
            Outcome = BDDTestOutcome.None
        };

        object scenarioInstance = null!;
        var previousStepFailed = false;
        var stepIndex = 0;

        try {
            scenarioInstance = Activator.CreateInstance(scenarioType)!;
            InjectScenarioContext(scenarioInstance, scenarioType);

            ExecuteHooks(scenarioInstance, scenarioType, typeof(BeforeScenarioAttribute), messageCallback);

            foreach (var step in steps) {
                stepIndex++;

                if (previousStepFailed && step.SkipOnPreviousFailure) {
                    var skippedResult = new StepResult {
                        StepType = step.Type.ToString(),
                        Description = step.Description!,
                        Success = false,
                        ErrorMessage = "⊘ Pulado devido a falha anterior"
                    };
                    result.StepResults.Add(skippedResult);
                    messageCallback?.Invoke($"  ⊘ [{stepIndex}/{steps.Length}] {step.Type}: {step.Description} (pulado)");
                    continue;
                }

                var stepResult = ExecuteStep(scenarioInstance, scenarioType, step, messageCallback!, stepIndex, steps.Length);
                result.StepResults.Add(stepResult);

                if (!stepResult.Success) {
                    previousStepFailed = true;
                    result.Outcome = BDDTestOutcome.Failed;

                    result.ErrorMessage = BuildDetailedErrorMessage(
                        scenarioType,
                        step,
                        stepResult,
                        stepIndex,
                        steps.Length);

                    result.ErrorStackTrace = stepResult.ErrorStackTrace;

                    if (step.Type != StepType.AfterScenario)
                        break;
                }
            }

            if (result.Outcome != BDDTestOutcome.Failed)
                result.Outcome = BDDTestOutcome.Passed;
        }
        catch (Exception ex) {
            result.Outcome = BDDTestOutcome.Failed;
            result.ErrorMessage = $"❌ ERRO FATAL durante execução do cenário:\n{ex.Message}";
            result.ErrorStackTrace = ex.StackTrace;
        }
        finally {
            if (scenarioInstance != null) {
                ExecuteHooks(scenarioInstance, scenarioType, typeof(AfterScenarioAttribute), messageCallback);
            }

            result.EndTime = DateTimeOffset.Now;
            (scenarioInstance as IDisposable)?.Dispose();
        }

        return result;
    }

    /// <summary>
    /// Executa um step individual do cenário.
    /// </summary>
    /// <param name="scenarioInstance">Instância do cenário.</param>
    /// <param name="scenarioType">Tipo do cenário.</param>
    /// <param name="step">Informações do step.</param>
    /// <param name="messageCallback">Callback para mensagens.</param>
    /// <param name="stepIndex">Índice do step atual.</param>
    /// <param name="totalSteps">Total de steps no cenário.</param>
    /// <returns>Resultado da execução do step.</returns>
    private static StepResult ExecuteStep(
        object scenarioInstance,
        Type scenarioType,
        StepInfo step,
        Action<string> messageCallback,
        int stepIndex,
        int totalSteps) {
        var stepResult = new StepResult {
            StepType = step.Type.ToString(),
            Description = step.Description!
        };

        var stopwatch = Stopwatch.StartNew();

        try {
            var method = scenarioType.GetMethod(step.MethodName!, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException(
                    "❌ MÉTODO NÃO ENCONTRADO\n" +
                    $"   Step: [{step.Type}] {step.Description}\n" +
                    $"   Método esperado: {step.MethodName}\n" +
                    $"   Classe: {scenarioType.FullName}");
            var parameters = ExtractParameters(method, step);

            var paramDisplay = parameters.Length > 0
                ? $" [{string.Join(", ", parameters.Select(p => $"'{p}'"))}]"
                : "";

            messageCallback?.Invoke($"  → [{stepIndex}/{totalSteps}] {step.Type}: {step.Description}");

            var returnValue = method.Invoke(scenarioInstance, parameters);

            if (returnValue is Task task)
                task.GetAwaiter().GetResult();

            stepResult.Success = true;
            messageCallback?.Invoke($"  ✓ [{stepIndex}/{totalSteps}] {step.Type}: {step.Description} ({stopwatch.ElapsedMilliseconds}ms)");
        }
        catch (TargetInvocationException ex) {
            stopwatch.Stop();
            stepResult.Success = false;
            var innerException = ex.InnerException ?? ex;

            stepResult.ErrorMessage = innerException.Message;
            stepResult.ErrorStackTrace = innerException.StackTrace;

            messageCallback?.Invoke($"  ✗ [{stepIndex}/{totalSteps}] {step.Type}: {step.Description}");
            messageCallback?.Invoke($"      ⮡ {innerException.GetType().Name}: {innerException.Message}");
        }
        catch (Exception ex) {
            stopwatch.Stop();
            stepResult.Success = false;
            stepResult.ErrorMessage = ex.Message;
            stepResult.ErrorStackTrace = ex.StackTrace;

            messageCallback?.Invoke($"  ✗ [{stepIndex}/{totalSteps}] {step.Type}: {step.Description}");
            messageCallback?.Invoke($"      ⮡ {ex.GetType().Name}: {ex.Message}");
        }
        finally {
            stopwatch.Stop();
            stepResult.Duration = stopwatch.Elapsed;
        }

        return stepResult;
    }

    /// <summary>
    /// Extrai e converte os parâmetros necessários para o método do step.
    /// </summary>
    /// <param name="method">Método alvo.</param>
    /// <param name="step">Informações do step.</param>
    /// <returns>Array de parâmetros convertidos.</returns>
    private static object[] ExtractParameters(MethodInfo method, StepInfo step) {
        var methodParams = method.GetParameters();

        if (methodParams.Length == 0)
            return [];

        // Se já tem parâmetros extraídos
        if (step.Parameters?.Length > 0)
            return ConvertParameters(step.Parameters, methodParams);

        // Se tem texto Gherkin, extrai via regex
        if (!string.IsNullOrEmpty(step.GherkinStepText)) {
            var stepAttr = GetStepAttribute(method);

            if (stepAttr != null && !string.IsNullOrEmpty(stepAttr.Pattern)) {
                if (stepAttr.Matches(step.GherkinStepText, out var match)) {
                    var extractedParams = StepAttribute.ExtractParameters(match);
                    return ConvertParameters(extractedParams, methodParams);
                }
            }

            // Se não tem pattern, tenta extrair do Description como regex
            var regex = new Regex(step.Description!, RegexOptions.IgnoreCase);
            var match2 = regex.Match(step.GherkinStepText);

            if (match2.Success && match2.Groups.Count > 1) {
                var values = new object[match2.Groups.Count - 1];
                for (var i = 1; i < match2.Groups.Count; i++) {
                    values[i - 1] = match2.Groups[i].Value;
                }
                return ConvertParameters(values, methodParams);
            }
        }

        return [];
    }

    /// <summary>
    /// Converte valores para os tipos esperados dos parâmetros do método.
    /// </summary>
    /// <param name="values">Valores a serem convertidos.</param>
    /// <param name="targetParams">Parâmetros de destino.</param>
    /// <returns>Array de valores convertidos.</returns>
    private static object[] ConvertParameters(object[] values, ParameterInfo[] targetParams) {
        var converted = new object[targetParams.Length];

        for (var i = 0; i < Math.Min(values.Length, targetParams.Length); i++) {
            var value = values[i]?.ToString();
            var targetType = targetParams[i].ParameterType;

            try {
                if (value == null) {
                    converted[i] = null!;
                }
                else if (targetType == typeof(string)) {
                    converted[i] = value;
                }
                else if (targetType == typeof(int)) {
                    converted[i] = Convert.ToInt32(value);
                }
                else if (targetType == typeof(decimal) && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultDecimal)) {
                    converted[i] = resultDecimal;
                }
                else if (targetType == typeof(double) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultDouble)) {
                    converted[i] = resultDouble;
                }
                else if (targetType == typeof(bool)) {
                    converted[i] = Convert.ToBoolean(value);
                }
                else if (targetType == typeof(DateTime)) {
                    converted[i] = Convert.ToDateTime(value);
                }
                else if (targetType == typeof(Guid) && Guid.TryParse(value, out var resultGuid)) {
                    converted[i] = resultGuid;
                }
                else {
                    converted[i] = Convert.ChangeType(value, targetType);
                }
            }
            catch (Exception ex) {
                throw new InvalidOperationException(
                    $"Não foi possível converter o parâmetro {i} ('{value}') para o tipo {targetType.Name}: {ex.Message}");
            }
        }

        return converted;
    }

    /// <summary>
    /// Obtém o atributo de step (Given, When, Then) do método, se existir.
    /// </summary>
    /// <param name="method">Método a ser inspecionado.</param>
    /// <returns>Instância de <see cref="StepAttribute"/> ou null.</returns>
    private static StepAttribute? GetStepAttribute(MethodInfo method) =>
               method.GetCustomAttribute<GivenAttribute>() as StepAttribute
            ?? method.GetCustomAttribute<WhenAttribute>() as StepAttribute
            ?? method.GetCustomAttribute<ThenAttribute>() as StepAttribute;

    /// <summary>
    /// Monta uma mensagem detalhada de erro para falhas em steps.
    /// </summary>
    /// <param name="scenarioType">Tipo do cenário.</param>
    /// <param name="failedStep">Step que falhou.</param>
    /// <param name="stepResult">Resultado do step.</param>
    /// <param name="stepIndex">Índice do step.</param>
    /// <param name="totalSteps">Total de steps.</param>
    /// <returns>Mensagem detalhada de erro.</returns>
    private static string BuildDetailedErrorMessage(
        Type scenarioType,
        StepInfo failedStep,
        StepResult stepResult,
        int stepIndex,
        int totalSteps) {
        var sb = new StringBuilder();

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"❌ FALHA NO STEP {stepIndex} DE {totalSteps}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();

        sb.AppendLine("📍 TIPO DO STEP:");
        sb.AppendLine($"   [{failedStep.Type}]");
        sb.AppendLine();

        sb.AppendLine("📝 DESCRIÇÃO:");
        sb.AppendLine($"   \"{failedStep.Description}\"");
        sb.AppendLine();

        sb.AppendLine("⚙️  MÉTODO:");
        sb.AppendLine($"   {scenarioType.Name}.{failedStep.MethodName}()");
        sb.AppendLine();
        sb.AppendLine($"Parameters   {string.Join(",", failedStep.Parameters?.ToString())}");

        sb.AppendLine("🔴 ERRO:");
        sb.AppendLine($"   {stepResult.ErrorMessage}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(stepResult.ErrorStackTrace)) {
            sb.AppendLine("📚 STACK TRACE:");
            var stackLines = stepResult.ErrorStackTrace.Split('\n')
                .Take(5)
                .Select(line => $"   {line.Trim()}");
            sb.AppendJoin("\n", stackLines).AppendLine();
            sb.AppendLine();
        }

        sb.AppendLine("⏱️  TEMPO ATÉ FALHA:");
        sb.AppendLine($"   {stepResult.Duration.TotalMilliseconds:F0}ms");
        sb.AppendLine();

        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        return sb.ToString();
    }

    /// <summary>
    /// Executa métodos marcados como hooks (BeforeScenario, AfterScenario).
    /// </summary>
    /// <param name="scenarioInstance">Instância do cenário.</param>
    /// <param name="scenarioType">Tipo do cenário.</param>
    /// <param name="hookAttributeType">Tipo do atributo do hook.</param>
    /// <param name="messageCallback">Callback para mensagens.</param>
    private static void ExecuteHooks(object scenarioInstance, Type scenarioType, Type hookAttributeType, Action<string> messageCallback) {
        var methods = scenarioType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(m => m.GetCustomAttribute(hookAttributeType) != null)
            .ToArray();

        foreach (var method in methods) {
            try {
                var hookName = hookAttributeType.Name.Replace("Attribute", "");
                messageCallback?.Invoke($"  🔧 Executando [{hookName}] {method.Name}");

                var returnValue = method.Invoke(scenarioInstance, null);

                if (returnValue is Task task) {
                    task.GetAwaiter().GetResult();
                }

                messageCallback?.Invoke($"  ✓ [{hookName}] {method.Name} concluído");
            }
            catch (Exception ex) {
                var innerException = ex is TargetInvocationException tie ? tie.InnerException : ex;
                messageCallback?.Invoke($"  ❌ Erro no hook {method.Name}: {innerException?.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Injeta o contexto do cenário na instância, se aplicável.
    /// </summary>
    /// <param name="scenarioInstance">Instância do cenário.</param>
    /// <param name="scenarioType">Tipo do cenário.</param>
    private static void InjectScenarioContext(object scenarioInstance, Type scenarioType) {
        var contextProperty = scenarioType.GetProperty("Context", BindingFlags.Public | BindingFlags.Instance);

        if (contextProperty?.PropertyType == typeof(ScenarioContextAttribute)) {
            var context = new ScenarioContextAttribute();
            var scenarioAttr = scenarioType.GetCustomAttribute<ScenarioAttribute>();

            if (scenarioAttr != null) {
                context.ScenarioName = scenarioAttr.Description;
                context.Feature = scenarioAttr.Feature;
            }

            contextProperty.SetValue(scenarioInstance, context);
        }
    }
}