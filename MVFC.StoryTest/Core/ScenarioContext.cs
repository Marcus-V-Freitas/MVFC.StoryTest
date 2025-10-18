namespace MVFC.StoryTest.Core;

/// <summary>
/// Gerencia o contexto de execução de um cenário, incluindo hooks de ciclo de vida.
/// </summary>
/// <typeparam name="T">Tipo do contexto do cenário.</typeparam>
internal sealed class ScenarioContext<T>
{
    public T? Instance { get; set; }

    public Action<T>? BeforeScenarioSync;
    public Func<T, Task>? BeforeScenarioAsync;
    public Action<T>? AfterScenarioSync;
    public Func<T, Task>? AfterScenarioAsync;

    public Action<T>? BeforeStepSync;
    public Func<T, Task>? BeforeStepAsync;
    public Action<T>? AfterStepSync;
    public Func<T, Task>? AfterStepAsync;

    /// <summary>
    /// Executa o hook de pré-cenário.
    /// </summary>
    public void InvokeBeforeScenario(T ctx)
    {
        if (BeforeScenarioAsync != null)
            BeforeScenarioAsync(ctx).GetAwaiter().GetResult();
        else
            BeforeScenarioSync?.Invoke(ctx);
    }

    /// <summary>
    /// Executa o hook de pós-cenário.
    /// </summary>
    public void InvokeAfterScenario(T ctx)
    {
        if (AfterScenarioAsync != null)
            AfterScenarioAsync(ctx).GetAwaiter().GetResult();
        else
            AfterScenarioSync?.Invoke(ctx);
    }

    /// <summary>
    /// Executa o hook de pré-passo.
    /// </summary>
    public void InvokeBeforeStep(T ctx)
    {
        if (BeforeStepAsync != null)
            BeforeStepAsync(ctx).GetAwaiter().GetResult();
        else
            BeforeStepSync?.Invoke(ctx);
    }

    /// <summary>
    /// Executa o hook de pós-passo.
    /// </summary>
    public void InvokeAfterStep(T ctx)
    {
        if (AfterStepAsync != null)
            AfterStepAsync(ctx).GetAwaiter().GetResult();
        else
            AfterStepSync?.Invoke(ctx);
    }
}