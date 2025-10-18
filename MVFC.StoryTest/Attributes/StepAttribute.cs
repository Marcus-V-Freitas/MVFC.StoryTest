namespace MVFC.StoryTest.Attributes;

/// <summary>
/// Base para atributos de passos BDD ("Given", "When", "Then").
/// </summary>
public abstract class StepAttribute(string description) : Attribute {
    /// <summary>
    /// Descrição do passo.
    /// </summary>
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    /// <summary>
    /// Padrão regex para correspondência do passo.
    /// </summary>
    public string Pattern { get; set; } = description;

    /// <summary>
    /// Ordem de execução do passo.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Indica se o passo deve ser pulado em caso de falha anterior.
    /// </summary>
    public bool SkipOnPreviousFailure { get; set; } = false;

    /// <summary>
    /// Verifica se o texto do passo corresponde ao padrão.
    /// </summary>
    /// <param name="stepText">Texto do passo.</param>
    /// <param name="match">Match encontrado.</param>
    /// <returns>True se corresponder, senão false.</returns>
    public bool Matches(string stepText, out Match match) {
        match = null!;

        if (string.IsNullOrEmpty(Pattern))
            return false;

        var regex = new Regex(Pattern, RegexOptions.IgnoreCase);
        match = regex.Match(stepText);

        return match.Success;
    }

    /// <summary>
    /// Extrai parâmetros do match regex.
    /// </summary>
    /// <param name="match">Match do regex.</param>
    /// <returns>Array de parâmetros extraídos.</returns>
    public static object[] ExtractParameters(Match match) {
        if (match?.Success != true)
            return [];

        var parameters = new List<object>();

        for (var i = 1; i < match.Groups.Count; i++) {
            var value = match.Groups[i].Value;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
                parameters.Add(decimalValue);
            else if (int.TryParse(value, out var intValue))
                parameters.Add(intValue);
            else if (bool.TryParse(value, out var boolValue))
                parameters.Add(boolValue);
            else
                parameters.Add(value);
        }

        return [.. parameters];
    }
}