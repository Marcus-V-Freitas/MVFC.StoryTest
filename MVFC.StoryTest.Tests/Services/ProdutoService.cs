namespace MVFC.StoryTest.Tests.Services;

public sealed class ProdutoService {
    private readonly Dictionary<Guid, Produto> _produtos = [];

    public Produto CriarProduto(CriarProdutoRequest request) {
        request.Validate();

        var produto = new Produto(
            Id: Guid.NewGuid(),
            Nome: request.Nome,
            Preco: request.Preco
        );

        _produtos[produto.Id] = produto;

        return produto;
    }

    public Produto? ObterProdutoPorId(Guid id) =>
        _produtos.TryGetValue(id, out var produto) ? produto : null;

    public IEnumerable<Produto> ListarTodos() =>
        _produtos.Values;

    public void LimparProdutos() =>
        _produtos.Clear();
}