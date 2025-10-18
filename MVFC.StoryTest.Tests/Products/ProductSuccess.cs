namespace MVFC.StoryTest.Tests.Products;

[StoryBinding("Product Registration", "Create a new product successfully")]
public sealed class ProductSuccess : ProductsSteps
{
    [Then("the product should be created successfully")]
    public void ThenTheProductShouldBeCreatedSuccessfully()
    {
        _exception.Should().BeNull("não deve ter ocorrido erro");
        _createdProduct.Should().NotBeNull();
        _createdProduct.Id.Should().NotBeEmpty();
        _createdProduct.Nome.Should().Be(_request?.Nome);
        _createdProduct.Preco.Should().Be(_request?.Preco);
    }

    [Then("I should be able to retrieve this product by its ID")]
    public void ThenIShouldBeAbleToRetrieveThisProductByItsID()
    {
        var retrievedProduct = _service?.ObterProdutoPorId(_createdProduct!.Id);

        retrievedProduct.Should().NotBeNull();
        retrievedProduct.Id.Should().Be(_createdProduct!.Id);
        retrievedProduct.Nome.Should().Be(_createdProduct.Nome);
        retrievedProduct.Preco.Should().Be(_createdProduct.Preco);
    }
}