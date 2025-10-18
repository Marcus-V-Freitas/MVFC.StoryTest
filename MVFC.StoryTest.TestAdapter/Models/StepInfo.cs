namespace MVFC.StoryTest.TestAdapter.Models;

/// <summary>
/// Representa as informações de um step (passo) de um cenário BDD.
/// </summary>
public sealed class StepInfo
{
    /// <summary>
    /// Tipo do step (Given, When, Then, etc).
    /// </summary>
    internal StepType Type { get; set; }

    /// <summary>
    /// Descrição do step.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Nome do método associado ao step.
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Ordem de execução do step dentro do cenário.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indica se o step deve ser pulado em caso de falha anterior.
    /// </summary>
    public bool SkipOnPreviousFailure { get; set; }

    /// <summary>
    /// Texto original do step no Gherkin.
    /// </summary>
    public string? GherkinStepText { get; set; }

    /// <summary>
    /// Parâmetros do step.
    /// </summary>
    public object[]? Parameters { get; set; }
}