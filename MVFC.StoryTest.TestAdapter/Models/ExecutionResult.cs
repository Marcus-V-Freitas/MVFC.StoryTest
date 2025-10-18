namespace MVFC.StoryTest.TestAdapter.Models;

/// <summary>
/// Representa o resultado da execução de um cenário BDD.
/// </summary>
public sealed class ExecutionResult
{
    /// <summary>
    /// Data/hora de início da execução.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Data/hora de término da execução.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Resultado da execução do cenário.
    /// </summary>
    public BDDTestOutcome Outcome { get; set; }

    /// <summary>
    /// Mensagem de erro, caso tenha ocorrido.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace do erro, caso tenha ocorrido.
    /// </summary>
    public string? ErrorStackTrace { get; set; }

    /// <summary>
    /// Lista de resultados dos steps executados.
    /// </summary>
    public List<StepResult> StepResults { get; set; } = [];

    /// <summary>
    /// Duração total da execução.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;
}