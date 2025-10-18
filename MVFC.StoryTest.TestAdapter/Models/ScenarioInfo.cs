namespace MVFC.StoryTest.TestAdapter.Models;

/// <summary>
/// Representa as informações de um cenário BDD, incluindo nome, descrição, tags e steps.
/// </summary>
public sealed class ScenarioInfo
{
    /// <summary>
    /// Nome totalmente qualificado do cenário.
    /// </summary>
    public string FullyQualifiedName { get; set; } = default!;

    /// <summary>
    /// Descrição do cenário.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Nome da feature à qual o cenário pertence.
    /// </summary>
    public string Feature { get; set; } = default!;

    /// <summary>
    /// Lista de tags associadas ao cenário.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Caminho do arquivo de origem do cenário.
    /// </summary>
    public string SourceFile { get; set; } = default!;

    /// <summary>
    /// Linha do arquivo onde o cenário está definido.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Caminho do arquivo de feature.
    /// </summary>
    public string FeatureFile { get; set; } = default!;

    /// <summary>
    /// Linha do arquivo de feature onde o cenário está definido.
    /// </summary>
    public int FeatureLine { get; set; }

    /// <summary>
    /// Lista de steps do cenário.
    /// </summary>
    public List<StepInfo> Steps { get; set; } = [];
}