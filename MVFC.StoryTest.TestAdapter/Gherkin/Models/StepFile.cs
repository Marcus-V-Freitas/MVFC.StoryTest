namespace MVFC.StoryTest.TestAdapter.Gherkin.Models;

/// <summary>
/// Representa um step (passo) de um cenário Gherkin.
/// </summary>
public sealed class StepFile
{
    /// <summary>
    /// Tipo do step (Given, When, Then, etc).
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Texto do step.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Número da linha no arquivo onde o step está definido.
    /// </summary>
    public int LineNumber { get; set; }
}