namespace MVFC.StoryTest.TestAdapter;

/// <summary>
/// Executor de testes BDD para o Visual Studio Test Platform.
/// Responsável por executar e gerenciar cenários de teste BDD descobertos.
/// </summary>
[ExtensionUri(ExecutorUriString)]
public sealed class BddTestExecutor : ITestExecutor {
    /// <summary>
    /// URI utilizada para registrar o executor de testes BDD.
    /// </summary>
    public const string ExecutorUriString = "executor://BddTestExecutor";

    /// <summary>
    /// Propriedade customizada para armazenar os steps serializados do cenário.
    /// </summary>
    public static readonly TestProperty StepsProperty =
        TestProperty.Register("BDD.Steps", "Steps", typeof(string), typeof(TestCase));

    private volatile bool _cancelled;

    /// <summary>
    /// Executa uma coleção de <see cref="TestCase"/>s.
    /// </summary>
    /// <param name="tests">Coleção de casos de teste a serem executados.</param>
    /// <param name="runContext">Contexto de execução.</param>
    /// <param name="frameworkHandle">Manipulador do framework </param>
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle) {
        _cancelled = false;

        if (tests is null || frameworkHandle is null)
            return;

        foreach (var testCase in tests) {
            if (_cancelled)
                break;

            RunTest(testCase, frameworkHandle);
        }
    }

    /// <summary>
    /// Executa todos os testes encontrados nas fontes especificadas.
    /// </summary>
    /// <param name="sources">Coleção de caminhos de origem dos testes.</param>
    /// <param name="runContext">Contexto de execução.</param>
    /// <param name="frameworkHandle">Manipulador do framework de teste.</param>
    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle) {
        _cancelled = false;

        if (sources is null || frameworkHandle is null)
            return;

        var testCases = new List<TestCase>();
        var discoverySink = new TestCaseDiscoverySink(testCases);

        var discoverer = new BddTestDiscoverer();
        discoverer.DiscoverTests(sources, runContext!, frameworkHandle, discoverySink);

        RunTests(testCases, runContext, frameworkHandle);
    }

    /// <summary>
    /// Solicita o cancelamento da execução dos testes em andamento.
    /// </summary>
    public void Cancel() =>
        _cancelled = true;

    /// <summary>
    /// Executa um único <see cref="TestCase"/> e registra o resultado.
    /// </summary>
    /// <param name="testCase">Caso de teste a ser executado.</param>
    /// <param name="frameworkHandle">Manipulador do framework de teste.</param>
    private static void RunTest(TestCase testCase, IFrameworkHandle frameworkHandle) {
        frameworkHandle.RecordStart(testCase);

        var testResult = new TestResult(testCase) {
            StartTime = DateTimeOffset.Now
        };

        try {
            var assembly = Assembly.LoadFrom(testCase.Source);

            var fullName = testCase.FullyQualifiedName;
            string typeName;

            var lastDot = fullName.LastIndexOf('.');
            typeName = lastDot > 0 && assembly.GetType(fullName[..lastDot]) != null
                ? fullName[..lastDot]
                : fullName;

            var scenarioType = assembly.GetType(typeName)
                ?? throw new InvalidOperationException($"Tipo '{testCase.FullyQualifiedName}' não encontrado");

            var stepsJson = testCase.GetPropertyValue(StepsProperty, string.Empty);
            StepInfo[] steps;

            if (string.IsNullOrEmpty(stepsJson)) {
                var discoverer = new HybridScenarioDiscoverer();
                var scenarios = HybridScenarioDiscoverer.DiscoverScenarios(assembly);
                var scenario = scenarios.FirstOrDefault(s => s.FullyQualifiedName == testCase.FullyQualifiedName);
                steps = scenario?.Steps.ToArray() ?? [];
            }
            else {
                steps = JsonSerializer.Deserialize<StepInfo[]>(stepsJson) ?? [];
            }

            var runner = new ScenarioRunner();
            var executionResult = ScenarioRunner.RunScenario(
                scenarioType,
                steps,
                message => frameworkHandle.SendMessage(TestMessageLevel.Informational, message));

            testResult.Outcome = executionResult.Outcome == BDDTestOutcome.Passed
                ? TestOutcome.Passed
                : TestOutcome.Failed;
            testResult.ErrorMessage = executionResult.ErrorMessage;
            testResult.ErrorStackTrace = executionResult.ErrorStackTrace;
            testResult.Duration = executionResult.Duration;
        }
        catch (Exception ex) {
            testResult.Outcome = TestOutcome.Failed;
            testResult.ErrorMessage = ex.Message;
            testResult.ErrorStackTrace = ex.StackTrace;
        }
        finally {
            testResult.EndTime = DateTimeOffset.Now;
            frameworkHandle.RecordResult(testResult);
            frameworkHandle.RecordEnd(testCase, testResult.Outcome);
        }
    }
}