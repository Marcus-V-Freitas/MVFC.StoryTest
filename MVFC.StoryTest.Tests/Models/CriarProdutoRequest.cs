namespace MVFC.StoryTest.Tests.Models;

public sealed record CriarProdutoRequest(string Nome, decimal Preco)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Nome))
            throw new ArgumentException("Nome do produto é obrigatório");

        if (Preco <= 0)
            throw new ArgumentException("Preco deve ser maior que zero");
    }
}