namespace MVFC.StoryTest.Primitives;

/// <summary>
/// Define os tipos de steps possíveis em um cenário de teste.
/// </summary>
internal enum StepType
{
    BeforeScenario = 0,
    Given = 1,
    When = 2,
    Then = 3,
    AfterScenario = 99
}