namespace MVFC.StoryTest.Core;

/// <summary>
/// Fornece métodos utilitários para criação de cenários de teste.
/// </summary>
public static class Story
{
    /// <summary>
    /// Cria um novo <see cref="ScenarioBuilder{T}"/> para o tipo de contexto especificado.
    /// </summary>
    /// <typeparam name="T">Tipo do contexto do cenário.</typeparam>
    /// <param name="title">Título do cenário.</param>
    /// <returns>Uma instância de <see cref="ScenarioBuilder{T}"/>.</returns>
    public static ScenarioBuilder<T> Scenario<T>(string title) where T : new() =>
        new(title);
}