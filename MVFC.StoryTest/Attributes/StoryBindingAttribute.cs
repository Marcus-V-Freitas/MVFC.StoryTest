namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Indica que uma classe está vinculada a uma feature e cenário específicos para testes de história.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class StoryBindingAttribute(string feature, string scenario) : Attribute
{
    /// <summary>
    /// Nome da feature à qual a classe está vinculada.
    /// </summary>
    public string Feature { get; set; } = feature;

    /// <summary>
    /// Nome do cenário ao qual a classe está vinculada.
    /// </summary>
    public string Scenario { get; set; } = scenario;
}
