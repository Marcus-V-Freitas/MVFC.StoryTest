namespace MVFC.StoryTest.Core;

/// <summary>
/// Armazena dados de contexto associados a um cenário de teste, permitindo o armazenamento e recuperação de valores por chave.
/// </summary>
public sealed class ScenarioContextAttribute
{
    private readonly Dictionary<string, object> _data = [];

    /// <summary>
    /// Nome do cenário associado ao contexto.
    /// </summary>
    public string ScenarioName { get; set; } = default!;

    /// <summary>
    /// Nome da feature associada ao contexto.
    /// </summary>
    public string Feature { get; set; } = default!;

    /// <summary>
    /// Define um valor no contexto para a chave especificada.
    /// </summary>
    /// <typeparam name="T">Tipo do valor a ser armazenado.</typeparam>
    /// <param name="key">Chave de identificação.</param>
    /// <param name="value">Valor a ser armazenado.</param>
    public void Set<T>(string key, T value) => _data[key] = value!;

    /// <summary>
    /// Obtém o valor associado à chave especificada.
    /// </summary>
    /// <typeparam name="T">Tipo do valor esperado.</typeparam>
    /// <param name="key">Chave de identificação.</param>
    /// <returns>Valor associado à chave.</returns>
    /// <exception cref="KeyNotFoundException">Se a chave não existir no contexto.</exception>
    public T Get<T>(string key)
    {
        if (_data.TryGetValue(key, out var value))
            return (T)value;
        throw new KeyNotFoundException($"Chave '{key}' não encontrada no contexto.");
    }

    /// <summary>
    /// Tenta obter o valor associado à chave especificada.
    /// </summary>
    /// <typeparam name="T">Tipo do valor esperado.</typeparam>
    /// <param name="key">Chave de identificação.</param>
    /// <param name="value">Valor de saída, se encontrado.</param>
    /// <returns>True se a chave existir; caso contrário, false.</returns>
    public bool TryGet<T>(string key, out T value)
    {
        if (_data.TryGetValue(key, out var obj))
        {
            value = (T)obj;
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>
    /// Verifica se o contexto contém a chave especificada.
    /// </summary>
    /// <param name="key">Chave de identificação.</param>
    /// <returns>True se a chave existir; caso contrário, false.</returns>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Limpa todos os dados armazenados no contexto.
    /// </summary>
    public void Clear() => _data.Clear();
}