namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Indica que uma classe representa um cenário de teste, permitindo a definição de descrição, feature e tags associadas.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ScenarioAttribute(string description) : Attribute
{
    /// <summary>
    /// Descrição do cenário de teste.
    /// </summary>
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    /// <summary>
    /// Nome da feature à qual o cenário pertence.
    /// </summary>
    public string Feature { get; set; } = default!;

    /// <summary>
    /// Lista de tags associadas ao cenário.
    /// </summary>
    public string[] Tags { get; set; } = [];
}