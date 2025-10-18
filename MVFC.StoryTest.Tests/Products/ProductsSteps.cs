namespace MVFC.StoryTest.Tests.Products;

public abstract class ProductsSteps {
    protected ProdutoService? _service;
    protected CriarProdutoRequest? _request;
    protected Produto? _createdProduct;
    protected Exception? _exception;

    [BeforeScenario]
    public void Setup() =>
        _service = new ProdutoService();

    [Given("I provide the data for a new product with name \"(.*)\" and price ([\\d.-]+(?:\\.[\\d]+)?)")]
    public void GivenIProvideTheDataForANewProduct(string name, decimal price) =>
        _request = new CriarProdutoRequest(name, price);

    [When("I send a request to create the product")]
    public void WhenISendARequestToCreateTheProduct() {
        try {
            _createdProduct = _service?.CriarProduto(_request!);
            _exception = null;
        }
        catch (Exception ex) {
            _exception = ex;
        }
    }

    [AfterScenario]
    public void Cleanup() =>
        _service?.LimparProdutos();
}