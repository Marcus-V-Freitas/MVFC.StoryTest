namespace MVFC.StoryTest.Core;

/// <summary>
/// Builder enxuto: só cria a definição do cenário (não executa nem faz logging).
/// </summary>
public sealed class ScenarioBuilder<T>(string title) where T : new()
{
    private readonly string _title = title ?? throw new ArgumentNullException(nameof(title));
    private readonly ScenarioContext<T> _ctx = new();
    private readonly List<Step<T>> _steps = [];

    /// <summary>
    /// Inicializa o contexto do cenário com uma nova instância de <typeparamref name="T"/>.
    /// </summary>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> WithContext()
    {
        _ctx.Instance = new T();
        return this;
    }

    /// <summary>
    /// Inicializa o contexto do cenário com uma instância fornecida de <typeparamref name="T"/>.
    /// </summary>
    /// <param name="instance">Instância do contexto.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> WithContext(T instance)
    {
        _ctx.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        return this;
    }

    /// <summary>
    /// Define uma ação síncrona a ser executada antes do cenário.
    /// </summary>
    /// <param name="action">Ação a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> BeforeScenario(Action<T> action)
    {
        _ctx.BeforeScenarioSync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação síncrona a ser executada após o cenário.
    /// </summary>
    /// <param name="action">Ação a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> AfterScenario(Action<T> action)
    {
        _ctx.AfterScenarioSync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação síncrona a ser executada antes de cada step.
    /// </summary>
    /// <param name="action">Ação a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> BeforeStep(Action<T> action)
    {
        _ctx.BeforeStepSync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação síncrona a ser executada após cada step.
    /// </summary>
    /// <param name="action">Ação a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> AfterStep(Action<T> action)
    {
        _ctx.AfterStepSync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação assíncrona a ser executada antes do cenário.
    /// </summary>
    /// <param name="action">Ação assíncrona a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> BeforeScenario(Func<T, Task> action)
    {
        _ctx.BeforeScenarioAsync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação assíncrona a ser executada após o cenário.
    /// </summary>
    /// <param name="action">Ação assíncrona a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> AfterScenario(Func<T, Task> action)
    {
        _ctx.AfterScenarioAsync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação assíncrona a ser executada antes de cada step.
    /// </summary>
    /// <param name="action">Ação assíncrona a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> BeforeStep(Func<T, Task> action)
    {
        _ctx.BeforeStepAsync = action;
        return this;
    }

    /// <summary>
    /// Define uma ação assíncrona a ser executada após cada step.
    /// </summary>
    /// <param name="action">Ação assíncrona a ser executada.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> AfterStep(Func<T, Task> action)
    {
        _ctx.AfterStepAsync = action;
        return this;
    }

    /// <summary>
    /// Adiciona um step do tipo "Given" síncrono ao cenário.
    /// </summary>
    /// <param name="name">Nome do step.</param>
    /// <param name="step">Ação do step.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> Given(string name, Action<T> step)
    {
        _steps.Add(new Step<T>(StepType.Given, name, step));
        return this;
    }

    /// <summary>
    /// Adiciona um step do tipo "When" síncrono ao cenário.
    /// </summary>
    /// <param name="name">Nome do step.</param>
    /// <param name="step">Ação do step.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> When(string name, Action<T> step)
    {
        _steps.Add(new Step<T>(StepType.When, name, step));
        return this;
    }

    /// <summary>
    /// Adiciona um step do tipo "Then" síncrono ao cenário.
    /// </summary>
    /// <param name="name">Nome do step.</param>
    /// <param name="step">Ação do step.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> Then(string name, Action<T> step)
    {
        _steps.Add(new Step<T>(StepType.Then, name, step));
        return this;
    }

    /// <summary>
    /// Adiciona um step do tipo "Given" assíncrono ao cenário.
    /// </summary>
    /// <param name="name">Nome do step.</param>
    /// <param name="step">Ação assíncrona do step.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> Given(string name, Func<T, Task> step)
    {
        _steps.Add(new Step<T>(StepType.Given, name, step)); return this;
    }

    /// <summary>
    /// Adiciona um step do tipo "When" assíncrono ao cenário.
    /// </summary>
    /// <param name="name">Nome do step.</param>
    /// <param name="step">Ação assíncrona do step.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> When(string name, Func<T, Task> step)
    {
        _steps.Add(new Step<T>(StepType.When, name, step)); return this;
    }

    /// <summary>
    /// Adiciona um step do tipo "Then" assíncrono ao cenário.
    /// </summary>
    /// <param name="name">Nome do step.</param>
    /// <param name="step">Ação assíncrona do step.</param>
    /// <returns>O próprio builder para encadeamento.</returns>
    public ScenarioBuilder<T> Then(string name, Func<T, Task> step)
    {
        _steps.Add(new Step<T>(StepType.Then, name, step)); return this;
    }

    /// <summary>
    /// Cria a definição do cenário a partir do builder.
    /// </summary>
    /// <returns>Uma instância de <see cref="ScenarioDefinition{T}"/>.</returns>
    internal ScenarioDefinition<T> Build()
    {
        var stepsCopy = new List<Step<T>>(_steps);
        var ctx = _ctx;
        return new ScenarioDefinition<T>(_title, ctx, stepsCopy);
    }
}