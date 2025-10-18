namespace MVFC.StoryTest.TestAdapter.Models;

/// <summary>
/// Representa o resultado da execução de um step de cenário BDD.
/// </summary>
public sealed class StepResult
{
    /// <summary>
    /// Tipo do step executado.
    /// </summary>
    public string StepType { get; set; } = default!;

    /// <summary>
    /// Descrição do step executado.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Indica se o step foi executado com sucesso.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem de erro, caso tenha ocorrido.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace do erro, caso tenha ocorrido.
    /// </summary>
    public string? ErrorStackTrace { get; set; }

    /// <summary>
    /// Duração da execução do step.
    /// </summary>
    public TimeSpan Duration { get; set; }
}