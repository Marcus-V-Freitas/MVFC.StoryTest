namespace MVFC.StoryTest.Runner;

/// <summary>
/// Classe responsável por executar um ScenarioDefinition: invoca hooks, executa steps
/// e faz a apresentação (console colorido). Totalmente separada do builder.
/// </summary>
public static class StoryRunner
{
    /// <summary>
    /// Executa um cenário de forma síncrona, invocando hooks e steps conforme definidos.
    /// </summary>
    /// <typeparam name="T">Tipo do contexto do cenário.</typeparam>
    public static void Run<T>(this ScenarioBuilder<T> scenarioBuilder) where T : new()
    {
        ArgumentNullException.ThrowIfNull(scenarioBuilder);

        var scenario = scenarioBuilder.Build();
        var ctx = EnsureContext(scenario);

        WriteHeader(scenario.Title);

        ctx.InvokeBeforeScenario(ctx.Instance!);

        foreach (var step in scenario.Steps)
        {
            ctx.InvokeBeforeStep(ctx.Instance!);
            RunStepSync(step, ctx.Instance!);
            ctx.InvokeAfterStep(ctx.Instance!);
        }

        ctx.InvokeAfterScenario(ctx.Instance!);

        WriteFooter();
    }

    /// <summary>
    /// Executa um cenário de forma assíncrona, invocando hooks e steps conforme definidos.
    /// </summary>
    /// <typeparam name="T">Tipo do contexto do cenário.</typeparam>
    /// <param name="scenarioBuilder">Builder do cenário a ser executado.</param>
    /// <returns>Uma <see cref="Task"/> que representa a execução assíncrona.</returns>
    public static async Task RunAsync<T>(this ScenarioBuilder<T> scenarioBuilder) where T : new()
    {
        ArgumentNullException.ThrowIfNull(scenarioBuilder);

        var scenario = scenarioBuilder.Build();
        var ctx = EnsureContext(scenario);

        WriteHeader(scenario.Title);

        if (ctx.BeforeScenarioAsync != null)
            await ctx.BeforeScenarioAsync(ctx.Instance!);
        else
            ctx.BeforeScenarioSync?.Invoke(ctx.Instance!);

        foreach (var step in scenario.Steps)
        {
            if (ctx.BeforeStepAsync != null)
                await ctx.BeforeStepAsync(ctx.Instance!);
            else
                ctx.BeforeStepSync?.Invoke(ctx.Instance!);

            await RunStepAsync(step, ctx.Instance!);

            if (ctx.AfterStepAsync != null)
                await ctx.AfterStepAsync(ctx.Instance!);
            else
                ctx.AfterStepSync?.Invoke(ctx.Instance!);
        }

        if (ctx.AfterScenarioAsync != null)
            await ctx.AfterScenarioAsync(ctx.Instance!);
        else
            ctx.AfterScenarioSync?.Invoke(ctx.Instance!);

        WriteFooter();
    }

    /// <summary>
    /// Garante que o contexto do cenário está inicializado.
    /// </summary>
    /// <typeparam name="T">Tipo do contexto.</typeparam>
    /// <param name="scenario">Definição do cenário.</param>
    /// <returns>O contexto do cenário.</returns>
    private static ScenarioContext<T> EnsureContext<T>(ScenarioDefinition<T> scenario) where T : new()
    {
        var ctx = scenario.Context;
        ctx.Instance ??= new T();

        return ctx;
    }

    /// <summary>
    /// Executa um step de forma síncrona, exibindo o resultado no console.
    /// </summary>
    /// <typeparam name="T">Tipo do contexto.</typeparam>
    /// <param name="step">Step a ser executado.</param>
    /// <param name="ctx">Instância do contexto.</param>
    private static void RunStepSync<T>(Step<T> step, T ctx)
    {
        var prevColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{step.Type}: {step.Name}");
            Console.ForegroundColor = prevColor;

            try
            {
                step.ExecuteSync(ctx);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Passed");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }
        finally
        {
            Console.ForegroundColor = prevColor;
        }
    }

    /// <summary>
    /// Executa um step de forma assíncrona, exibindo o resultado no console.
    /// </summary>
    /// <typeparam name="T">Tipo do contexto.</typeparam>
    /// <param name="step">Step a ser executado.</param>
    /// <param name="ctx">Instância do contexto.</param>
    /// <returns>Uma <see cref="Task"/> que representa a execução assíncrona do step.</returns>
    private static async Task RunStepAsync<T>(Step<T> step, T ctx)
    {
        var prevColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{step.Type}: {step.Name}");
            Console.ForegroundColor = prevColor;

            try
            {
                await step.ExecuteAsync(ctx);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Passed");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }
        finally
        {
            Console.ForegroundColor = prevColor;
        }
    }

    /// <summary>
    /// Escreve o cabeçalho do cenário no console.
    /// </summary>
    /// <param name="title">Título do cenário.</param>
    private static void WriteHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Scenario: {title}");
        Console.ResetColor();
    }

    /// <summary>
    /// Escreve o rodapé do cenário no console.
    /// </summary>
    private static void WriteFooter() => Console.WriteLine();
}
