namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Atributo para marcar métodos como passos "When" em cenários BDD.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WhenAttribute : StepAttribute
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="WhenAttribute"/> com uma descrição.
    /// </summary>
    /// <param name="description">Descrição do passo.</param>
    public WhenAttribute(string description) : base(description) { }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="WhenAttribute"/> com uma descrição e um padrão regex.
    /// </summary>
    /// <param name="description">Descrição do passo.</param>
    /// <param name="pattern">Padrão regex para correspondência do passo.</param>
    public WhenAttribute(string description, string pattern) : base(description)
    {
        Pattern = pattern;
    }
}