namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Atributo para marcar métodos como passos "Then" em cenários BDD.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ThenAttribute : StepAttribute
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="ThenAttribute"/> com uma descrição.
    /// </summary>
    /// <param name="description">Descrição do passo.</param>
    public ThenAttribute(string description) : base(description) { }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ThenAttribute"/> com uma descrição e um padrão regex.
    /// </summary>
    /// <param name="description">Descrição do passo.</param>
    /// <param name="pattern">Padrão regex para correspondência do passo.</param>
    public ThenAttribute(string description, string pattern) : base(description)
    {
        Pattern = pattern;
    }
}