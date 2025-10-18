namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Atributo para marcar métodos a serem executados após o cenário.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AfterScenarioAttribute : Attribute
{
    /// <summary>
    /// Ordem de execução do método.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AfterScenarioAttribute"/>.
    /// </summary>
    public AfterScenarioAttribute() { Order = 0; }
}