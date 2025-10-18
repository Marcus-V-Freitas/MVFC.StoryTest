namespace MVFC.StoryTest.Tests.Products;

[StoryBinding("Product Registration", "Reject product creation with invalid price")]
public sealed class ProductsReject : ProductsSteps
{

    [Then("the request should fail with error \"(.*)\"")]
    public void ThenTheRequestShouldFailWithError(string expectedError)
    {
        _exception.Should().NotBeNull("deve ter ocorrido um erro de validação");
        _exception.Message.Should().Contain(expectedError);
        _createdProduct.Should().BeNull("o produto não deveria ter sido criado");
    }
}