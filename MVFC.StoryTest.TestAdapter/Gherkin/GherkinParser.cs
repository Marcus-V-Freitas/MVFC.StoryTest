namespace MVFC.StoryTest.TestAdapter.Gherkin;

/// <summary>
/// Responsável por fazer o parsing de arquivos Gherkin (.feature) e converter em objetos de domínio.
/// </summary>
public sealed class GherkinParser {
    /// <summary>
    /// Palavras-chave Gherkin reconhecidas para identificar tipos de passos.
    /// </summary>
    private static readonly string[] _keywords = ["Given", "When", "Then", "And", "But"];

    /// <summary>
    /// Realiza o parsing de um arquivo .feature e retorna um objeto <see cref="FeatureFile"/> representando seu conteúdo.
    /// </summary>
    /// <param name="filePath">Caminho do arquivo .feature a ser processado.</param>
    /// <returns>Objeto <see cref="FeatureFile"/> com as informações extraídas do arquivo.</returns>
    public static FeatureFile ParseFile(string filePath) {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        var feature = new FeatureFile { FilePath = filePath };

        ScenarioFile currentScenario = null!;
        var currentTags = new List<string>();

        for (var i = 0; i < lines.Length; i++) {
            var line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (line.StartsWith('@')) {
                currentTags.AddRange(line.Split(['@', ' '], StringSplitOptions.RemoveEmptyEntries));
                continue;
            }

            if (line.StartsWith("Feature:", StringComparison.OrdinalIgnoreCase)) {
                feature.Name = line["Feature:".Length..].Trim();
                feature.Tags = [.. currentTags];
                currentTags.Clear();
                continue;
            }

            if (line.StartsWith("Scenario:", StringComparison.OrdinalIgnoreCase)) {
                currentScenario = new ScenarioFile {
                    Name = line["Scenario:".Length..].Trim(),
                    Tags = [.. currentTags],
                    LineNumber = i + 1
                };
                feature.Scenarios.Add(currentScenario);
                currentTags.Clear();
                continue;
            }

            if (currentScenario != null) {
                if (TryParseStep(line, out var stepType, out var stepText)) {
                    currentScenario.Steps.Add(new StepFile {
                        Type = stepType,
                        Text = stepText,
                        LineNumber = i + 1
                    });
                }
            }
        }

        return feature;
    }

    /// <summary>
    /// Tenta identificar e extrair o tipo e o texto de um passo Gherkin a partir de uma linha.
    /// </summary>
    /// <param name="line">Linha do arquivo a ser analisada.</param>
    /// <param name="stepType">Retorna o tipo do passo (Given, When, Then, And, But).</param>
    /// <param name="stepText">Retorna o texto do passo.</param>
    /// <returns>Verdadeiro se a linha representa um passo válido; caso contrário, falso.</returns>
    private static bool TryParseStep(string line, out string stepType, out string stepText) {
        stepType = null!;
        stepText = null!;

        foreach (var keyword in _keywords) {
            if (line.StartsWith(keyword + " ", StringComparison.OrdinalIgnoreCase)) {
                stepType = keyword;
                stepText = line[keyword.Length..].Trim();
                return true;
            }
        }

        return false;
    }
}