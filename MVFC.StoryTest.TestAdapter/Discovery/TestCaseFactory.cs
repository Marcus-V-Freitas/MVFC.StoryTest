namespace MVFC.StoryTest.TestAdapter.Discovery;

/// <summary>
/// Fábrica para criação de instâncias de <see cref="TestCase"/> a partir de informações de cenário BDD.
/// </summary>
public static class TestCaseFactory
{
    /// <summary>
    /// Propriedade customizada para armazenar o nome da feature.
    /// </summary>
    private static readonly TestProperty FeatureProperty =
        TestProperty.Register("BDD.Feature", "Feature", typeof(string), typeof(TestCase));

    /// <summary>
    /// Propriedade customizada para armazenar as tags do cenário.
    /// </summary>
    private static readonly TestProperty TagsProperty =
        TestProperty.Register("BDD.Tags", "Tags", typeof(string), typeof(TestCase));

    /// <summary>
    /// Propriedade customizada para armazenar a quantidade de steps.
    /// </summary>
    private static readonly TestProperty StepsCountProperty =
        TestProperty.Register("BDD.StepsCount", "StepsCount", typeof(int), typeof(TestCase));

    /// <summary>
    /// Cria uma instância de <see cref="TestCase"/> a partir das informações do cenário.
    /// </summary>
    /// <param name="scenarioInfo">Informações do cenário.</param>
    /// <param name="source">Caminho do arquivo de origem.</param>
    /// <returns>Instância de <see cref="TestCase"/> configurada.</returns>
    public static TestCase CreateTestCase(ScenarioInfo scenarioInfo, string source)
    {
        var testCase = new TestCase(
            scenarioInfo.FullyQualifiedName,
            new Uri(BddTestExecutor.ExecutorUriString),
            source)
        {
            DisplayName = scenarioInfo.Description,
            LineNumber = scenarioInfo.LineNumber,
            CodeFilePath = scenarioInfo.SourceFile
        };

        if (!string.IsNullOrEmpty(scenarioInfo.Feature))
            testCase.SetPropertyValue(FeatureProperty, scenarioInfo.Feature);

        if (scenarioInfo.Tags.Count != 0)
        {
            testCase.SetPropertyValue(TagsProperty, string.Join(",", scenarioInfo.Tags));
            foreach (var tag in scenarioInfo.Tags)
                testCase.Traits.Add(new Trait("Category", tag));
        }

        if (!string.IsNullOrEmpty(scenarioInfo.FeatureFile))
        {
            testCase.Traits.Add("Feature File", scenarioInfo.FeatureFile);
            testCase.Traits.Add("Feature Line", scenarioInfo.FeatureLine.ToString());
        }

        testCase.SetPropertyValue(StepsCountProperty, scenarioInfo.Steps.Count);

        return testCase;
    }
}