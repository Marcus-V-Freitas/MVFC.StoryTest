namespace MVFC.StoryTest.TestAdapter;

/// <summary>
/// Descobridor de testes BDD para integração com o VSTest.
/// </summary>
[FileExtension(".dll")]
[FileExtension(".exe")]
[DefaultExecutorUri(BddTestExecutor.ExecutorUriString)]
public sealed class BddTestDiscoverer : ITestDiscoverer
{
    /// <summary>
    /// Descobre e registra os testes BDD encontrados nos assemblies fornecidos.
    /// </summary>
    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        logger.SendMessage(TestMessageLevel.Informational, "StoryTest: Iniciando descoberta...");

        foreach (var source in sources)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
                    continue;

                var assembly = Assembly.LoadFrom(source);

                if (!assembly.GetReferencedAssemblies()
                    .Any(a => string.Equals(a.Name, "MVFC.StoryTest", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var discoverer = new HybridScenarioDiscoverer();
                var scenarios = HybridScenarioDiscoverer.DiscoverScenarios(assembly);

                if (scenarios.Count == 0)
                    continue;

                logger.SendMessage(TestMessageLevel.Informational,
                    $"StoryTest: Encontrados {scenarios.Count} cenário(s) em {Path.GetFileName(source)}");

                foreach (var scenario in scenarios)
                {
                    var testCase = TestCaseFactory.CreateTestCase(scenario, source);
                    testCase.SetPropertyValue(BddTestExecutor.StepsProperty, JsonSerializer.Serialize(scenario.Steps));

                    discoverySink.SendTestCase(testCase);
                }
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Error,
                    $"StoryTest: Erro em {source}: {ex.Message}");
            }
        }
    }
}
