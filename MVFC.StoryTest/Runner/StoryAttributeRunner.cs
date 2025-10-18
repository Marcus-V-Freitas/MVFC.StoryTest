namespace MVFC.StoryTest.Runner;

/// <summary>
/// Executa cenários marcados com atributos BDD.
/// </summary>
public static class StoryAttributeRunner
{
    /// <summary>
    /// Executa um cenário a partir do tipo informado, invocando métodos marcados com atributos BDD.
    /// </summary>
    /// <param name="scenarioType">Tipo do cenário.</param>
    public static void RunScenario(Type scenarioType)
    {
        var instance = Activator.CreateInstance(scenarioType);

        void RunStep<TAttr>()
        {
            var methods = scenarioType.GetMethods()
                .Where(m => m.GetCustomAttribute(typeof(TAttr)) != null);
            foreach (var method in methods)
            {
                method.Invoke(instance, null);
            }
        }

        RunStep<BeforeScenarioAttribute>();
        RunStep<GivenAttribute>();
        RunStep<WhenAttribute>();
        RunStep<ThenAttribute>();
        RunStep<AfterScenarioAttribute>();
    }

    /// <summary>
    /// Executa um cenário do tipo <typeparamref name="TTestClass"/> invocando métodos marcados com atributos BDD.
    /// </summary>
    /// <typeparam name="TTestClass">Tipo do cenário.</typeparam>
    public static void RunScenario<TTestClass>() where TTestClass : new()
    {
        var instance = new TTestClass();
        var type = typeof(TTestClass);

        foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<BeforeScenarioAttribute>() != null))
            method.Invoke(instance, null);

        foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<GivenAttribute>() != null))
            method.Invoke(instance, null);

        foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<WhenAttribute>() != null))
            method.Invoke(instance, null);

        foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<ThenAttribute>() != null))
            method.Invoke(instance, null);

        foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<AfterScenarioAttribute>() != null))
            method.Invoke(instance, null);
    }
}