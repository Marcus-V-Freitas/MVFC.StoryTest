namespace MVFC.StoryTest.TestAdapter.Gherkin.Models;

/// <summary>
/// Representa um arquivo de feature Gherkin, contendo cenários e metadados.
/// </summary>
public sealed class FeatureFile
{
    /// <summary>
    /// Caminho do arquivo .feature.
    /// </summary>
    public string FilePath { get; set; } = default!;

    /// <summary>
    /// Nome da feature.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Lista de tags associadas à feature.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Lista de cenários presentes na feature.
    /// </summary>
    public List<ScenarioFile> Scenarios { get; set; } = [];
}