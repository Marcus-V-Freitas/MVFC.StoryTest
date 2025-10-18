namespace MVFC.StoryTest.TestAdapter.Gherkin.Models;

/// <summary>
/// Representa um cenário dentro de um arquivo de feature Gherkin.
/// </summary>
public sealed class ScenarioFile
{
    /// <summary>
    /// Nome do cenário.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Lista de tags associadas ao cenário.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Lista de steps pertencentes ao cenário.
    /// </summary>
    public List<StepFile> Steps { get; set; } = [];

    /// <summary>
    /// Número da linha no arquivo onde o cenário está definido.
    /// </summary>
    public int LineNumber { get; set; }
}