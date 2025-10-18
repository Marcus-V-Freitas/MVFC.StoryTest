namespace MVFC.StoryTest.TestAdapter.Discovery;

/// <summary>
/// Responsável por descobrir cenários de teste em assemblies, tanto baseados em classes quanto em arquivos feature.
/// </summary>
public sealed class HybridScenarioDiscoverer {
    /// <summary>
    /// Descobre todos os cenários em um assembly.
    /// </summary>
    /// <param name="assembly">Assembly a ser inspecionado.</param>
    /// <returns>Lista de cenários encontrados.</returns>
    public static List<ScenarioInfo> DiscoverScenarios(Assembly assembly) {
        var scenarios = new List<ScenarioInfo>();
        scenarios.AddRange(DiscoverClassBasedScenarios(assembly));
        scenarios.AddRange(DiscoverFeatureFileScenarios(assembly));
        return scenarios;
    }

    /// <summary>
    /// Descobre cenários baseados em classes.
    /// </summary>
    private static List<ScenarioInfo> DiscoverClassBasedScenarios(Assembly assembly) {
        var scenarios = new List<ScenarioInfo>();

        foreach (var type in assembly.GetTypes()) {
            var scenarioAttr = type.GetCustomAttribute<ScenarioAttribute>();
            if (scenarioAttr == null) continue;

            var scenarioInfo = CreateScenarioInfoFromType(type, scenarioAttr);
            scenarios.Add(scenarioInfo);
        }

        return scenarios;
    }

    /// <summary>
    /// Cria um objeto <see cref="ScenarioInfo"/> a partir de um tipo e atributo de cenário.
    /// </summary>
    private static ScenarioInfo CreateScenarioInfoFromType(Type type, ScenarioAttribute scenarioAttr) {
        var scenarioInfo = new ScenarioInfo {
            FullyQualifiedName = type.FullName!,
            Description = scenarioAttr.Description,
            Feature = scenarioAttr.Feature,
            Tags = scenarioAttr.Tags?.ToList() ?? [],
            Steps = DiscoverSteps(type)
        };

        TryGetSourceLocation(type, out var sourceFile, out var lineNumber);
        scenarioInfo.SourceFile = sourceFile!;
        scenarioInfo.LineNumber = lineNumber;

        return scenarioInfo;
    }

    /// <summary>
    /// Descobre cenários definidos em arquivos feature.
    /// </summary>
    private static List<ScenarioInfo> DiscoverFeatureFileScenarios(Assembly assembly) {
        var scenarios = new List<ScenarioInfo>();

        try {
            var featureFiles = GetFeatureFiles(assembly);

            foreach (var featureFile in featureFiles) {
                var feature = GherkinParser.ParseFile(featureFile);
                var bindingClasses = FindBindingClass(assembly, feature.Name);

                if (bindingClasses == null)
                    continue;

                scenarios.AddRange(CreateScenarioInfosFromFeature(feature, bindingClasses, featureFile));
            }
        }
        catch { }

        return scenarios;
    }

    /// <summary>
    /// Obtém todos os arquivos .feature do projeto.
    /// </summary>
    private static string[] GetFeatureFiles(Assembly assembly) {
        var assemblyLocation = assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        var projectRoot = Path.GetFullPath(Path.Combine(assemblyDirectory!, "..", "..", ".."));
        return Directory.GetFiles(projectRoot, "*.feature", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Cria objetos <see cref="ScenarioInfo"/> a partir de um arquivo feature e classes de binding.
    /// </summary>
    private static IEnumerable<ScenarioInfo> CreateScenarioInfosFromFeature(FeatureFile feature, IEnumerable<Type> bindingClasses, string featureFile) {
        foreach (var gherkinScenario in feature.Scenarios) {
            var bindingClass = bindingClasses.FirstOrDefault(x => x.GetCustomAttribute<StoryBindingAttribute>()?.Scenario == gherkinScenario.Name);

            if (bindingClass == null)
                continue;

            TryGetSourceLocation(bindingClass, out var sourceFile, out var lineNumber);

            yield return new ScenarioInfo {
                FullyQualifiedName = $"{bindingClass.FullName}.{SanitizeName(gherkinScenario.Name)}",
                Description = gherkinScenario.Name,
                Feature = feature.Name,
                Tags = [.. gherkinScenario.Tags.Union(feature.Tags)],
                Steps = MapGherkinStepsToMethods(bindingClass, gherkinScenario.Steps),
                FeatureFile = featureFile,
                FeatureLine = gherkinScenario.LineNumber,
                SourceFile = sourceFile!,
                LineNumber = lineNumber,
            };
        }
    }

    /// <summary>
    /// Encontra classes de binding para um feature.
    /// </summary>
    private static IEnumerable<Type> FindBindingClass(Assembly assembly, string featureName) =>
        assembly.GetTypes()
            .Where(t =>
                t.Name.Contains(SanitizeName(featureName), StringComparison.OrdinalIgnoreCase) ||
                t.GetCustomAttribute<StoryBindingAttribute>()?.Feature == featureName);

    /// <summary>
    /// Mapeia passos Gherkin para métodos das classes de binding.
    /// </summary>
    private static List<StepInfo> MapGherkinStepsToMethods(Type bindingClass, List<StepFile> gherkinSteps) {
        var stepInfos = new List<StepInfo>();
        var methods = bindingClass.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        StepType? previousStepType = null;

        foreach (var gherkinStep in gherkinSteps) {
            var method = FindMatchingMethod(methods, gherkinStep, out var match, out var stepAttr);

            if (method == null)
                continue;

            var stepType = GetStepType(gherkinStep.Type, ref previousStepType);

            object[] parameters = [];
            if (match?.Success == true && stepAttr != null) {
                parameters = StepAttribute.ExtractParameters(match);
            }

            stepInfos.Add(new StepInfo {
                Type = stepType,
                Description = gherkinStep.Text,
                MethodName = method.Name,
                Order = stepInfos.Count,
                GherkinStepText = gherkinStep.Text,
                Parameters = parameters
            });
        }

        return stepInfos;
    }

    /// <summary>
    /// Obtém o tipo do passo a partir do texto Gherkin.
    /// </summary>
    private static StepType GetStepType(string type, ref StepType? previousStepType) {
        if (type is "And" or "But") {
            return previousStepType ?? StepType.Given;
        }
        else {
            var stepType = type switch {
                "Given" => StepType.Given,
                "When" => StepType.When,
                "Then" => StepType.Then,
                _ => StepType.Given
            };
            previousStepType = stepType;
            return stepType;
        }
    }

    /// <summary>
    /// Encontra o método correspondente a um passo Gherkin.
    /// </summary>
    private static MethodInfo? FindMatchingMethod(
        MethodInfo[] methods,
        StepFile gherkinStep,
        out Match? match,
        out StepAttribute? stepAttribute) {
        match = null;
        stepAttribute = null;

        foreach (var method in methods) {
            var givenAttr = method.GetCustomAttribute<GivenAttribute>(inherit: true);
            var whenAttr = method.GetCustomAttribute<WhenAttribute>(inherit: true);
            var thenAttr = method.GetCustomAttribute<ThenAttribute>(inherit: true);

            var attrs = new StepAttribute[] { givenAttr!, whenAttr!, thenAttr! }
                .Where(a => a != null)
                .ToArray();

            foreach (var attr in attrs) {
                if (attr.Matches(gherkinStep.Text, out var m)) {
                    match = m;
                    stepAttribute = attr;
                    return method;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Descobre os passos de um tipo de cenário.
    /// </summary>
    private static List<StepInfo> DiscoverSteps(Type scenarioType) {
        var steps = new List<StepInfo>();

        foreach (var method in scenarioType.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
            var stepInfo = CreateStepInfoFromMethod(method);
            if (stepInfo != null) steps.Add(stepInfo);
        }

        return [.. steps.OrderBy(s => (int)s.Type).ThenBy(s => s.Order)];
    }

    /// <summary>
    /// Cria um <see cref="StepInfo"/> a partir de um método.
    /// </summary>
    private static StepInfo? CreateStepInfoFromMethod(MethodInfo method) {
        if (method.GetCustomAttribute<GivenAttribute>() is var given && given != null) {
            return new StepInfo {
                Type = StepType.Given,
                Description = given.Description,
                MethodName = method.Name,
                Order = given.Order,
                SkipOnPreviousFailure = given.SkipOnPreviousFailure
            };
        }
        if (method.GetCustomAttribute<WhenAttribute>() is var when && when != null) {
            return new StepInfo {
                Type = StepType.When,
                Description = when.Description,
                MethodName = method.Name,
                Order = when.Order,
                SkipOnPreviousFailure = when.SkipOnPreviousFailure
            };
        }
        if (method.GetCustomAttribute<ThenAttribute>() is var then && then != null) {
            return new StepInfo {
                Type = StepType.Then,
                Description = then.Description,
                MethodName = method.Name,
                Order = then.Order,
                SkipOnPreviousFailure = then.SkipOnPreviousFailure
            };
        }
        if (method.GetCustomAttribute<BeforeScenarioAttribute>() is var before && before != null) {
            return new StepInfo {
                Type = StepType.BeforeScenario,
                Description = "Setup",
                MethodName = method.Name,
                Order = before.Order
            };
        }
        if (method.GetCustomAttribute<AfterScenarioAttribute>() is var after && after != null) {
            return new StepInfo {
                Type = StepType.AfterScenario,
                Description = "Teardown",
                MethodName = method.Name,
                Order = after.Order
            };
        }
        return null;
    }

    /// <summary>
    /// Tenta obter a localização do arquivo fonte de um tipo.
    /// </summary>
    private static bool TryGetSourceLocation(Type type, out string? sourceFile, out int lineNumber) {
        sourceFile = null;
        lineNumber = 1;

        try {
            var assemblyPath = type.Assembly.Location;
            var directory = Path.GetDirectoryName(assemblyPath);
            if (directory == null) return false;

            var projectRoot = FindProjectRoot(directory);
            if (projectRoot == null) return false;

            var csFiles = Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories);
            var namespaceName = type.Namespace;
            var className = type.Name;

            foreach (var file in csFiles) {
                var content = File.ReadAllText(file);

                // Deve conter namespace e class
                if (!string.IsNullOrWhiteSpace(namespaceName) &&
                    content.Contains($"namespace {namespaceName}") &&
                    content.Contains($"class {className}")) {
                    sourceFile = file;
                    return true;
                }
            }
        }
        catch {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Sobe até encontrar pasta com .csproj.
    /// </summary>
    private static string? FindProjectRoot(string startDir) {
        var dir = new DirectoryInfo(startDir);

        while (dir != null) {
            if (dir.GetFiles("*.csproj").Length != 0)
                return dir.FullName;

            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>
    /// Remove caracteres não alfanuméricos do nome.
    /// </summary>
    private static string SanitizeName(string name) =>
        new([.. name.Where(char.IsLetterOrDigit)]);
}