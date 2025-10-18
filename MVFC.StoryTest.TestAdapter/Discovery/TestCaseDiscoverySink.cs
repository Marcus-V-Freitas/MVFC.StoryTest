namespace MVFC.StoryTest.TestAdapter.Discovery;

/// <summary>
/// Implementa <see cref="ITestCaseDiscoverySink"/> para coletar e armazenar casos de teste descobertos durante o processo de discovery.
/// </summary>
/// <param name="testCases">Lista onde os casos de teste descobertos serão adicionados.</param>
internal sealed class TestCaseDiscoverySink(List<TestCase> testCases) : ITestCaseDiscoverySink
{
    private readonly List<TestCase> _testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));

    /// <summary>
    /// Adiciona um <see cref="TestCase"/> descoberto à lista de casos de teste.
    /// </summary>
    /// <param name="discoveredTest">Caso de teste descoberto.</param>
    public void SendTestCase(TestCase discoveredTest)
    {
        if (discoveredTest is not null)
            _testCases.Add(discoveredTest);
    }
}