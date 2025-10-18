namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Atributo para marcar métodos a serem executados antes do cenário.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class BeforeScenarioAttribute : Attribute
{
    /// <summary>
    /// Ordem de execução do método.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="BeforeScenarioAttribute"/>.
    /// </summary>
    public BeforeScenarioAttribute() { Order = 0; }
}
