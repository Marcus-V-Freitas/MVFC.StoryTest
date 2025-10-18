namespace MVFC.StoryTest.Core;

/// <summary>
/// Representa um cenário montado (imutável por contrato).
/// </summary>
/// <typeparam name="T">Tipo do contexto do cenário.</typeparam>
internal sealed class ScenarioDefinition<T>(
    string title,
    ScenarioContext<T> context,
    IReadOnlyList<Step<T>> steps)
    where T : new()
{
    /// <summary>
    /// Título do cenário.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Contexto do cenário.
    /// </summary>
    public ScenarioContext<T> Context { get; } = context;

    /// <summary>
    /// Lista de passos do cenário.
    /// </summary>
    public IReadOnlyList<Step<T>> Steps { get; } = steps;
}