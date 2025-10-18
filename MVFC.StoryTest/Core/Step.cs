namespace MVFC.StoryTest.Core;

/// <summary>
/// Representa um passo de cenário BDD, podendo ser síncrono ou assíncrono.
/// </summary>
/// <typeparam name="T">Tipo do contexto do cenário.</typeparam>
internal sealed class Step<T>
{
    private readonly Action<T>? _sync;
    private readonly Func<T, Task>? _async;

    /// <summary>
    /// Tipo do passo (Given, When, Then, etc).
    /// </summary>
    public StepType Type { get; }

    /// <summary>
    /// Nome ou descrição do passo.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Indica se o passo é assíncrono.
    /// </summary>
    public bool IsAsync => _async != null;

    /// <summary>
    /// Inicializa um passo síncrono.
    /// </summary>
    /// <param name="type">Tipo do passo.</param>
    /// <param name="name">Nome do passo.</param>
    /// <param name="sync">Ação síncrona a ser executada.</param>
    public Step(StepType type, string name, Action<T> sync)
    {
        Type = type;
        Name = name;
        _sync = sync;
    }

    /// <summary>
    /// Inicializa um passo assíncrono.
    /// </summary>
    /// <param name="type">Tipo do passo.</param>
    /// <param name="name">Nome do passo.</param>
    /// <param name="asyncAction">Ação assíncrona a ser executada.</param>
    public Step(StepType type, string name, Func<T, Task> asyncAction)
    {
        Type = type;
        Name = name;
        _async = asyncAction;
    }

    /// <summary>
    /// Executa o passo de forma síncrona.
    /// </summary>
    /// <param name="ctx">Contexto do cenário.</param>
    public void ExecuteSync(T ctx)
    {
        if (_sync != null)
        {
            _sync(ctx);
            return;
        }

        if (_async != null)
        {
            _async(ctx).GetAwaiter().GetResult();
            return;
        }

        throw new InvalidOperationException("Step has no action.");
    }

    /// <summary>
    /// Executa o passo de forma assíncrona.
    /// </summary>
    /// <param name="ctx">Contexto do cenário.</param>
    /// <returns>Uma tarefa que representa a execução do passo.</returns>
    public Task ExecuteAsync(T ctx)
    {
        if (_async != null)
            return _async(ctx);

        _sync?.Invoke(ctx);

        return Task.CompletedTask;
    }
}