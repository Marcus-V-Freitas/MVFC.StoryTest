# MVFC.StoryTest

MVFC.StoryTest é um framework enxuto para definição e execução de cenários de teste (Story Tests) em .NET, com suporte a BDD (Behavior Driven Development), hooks, steps síncronos e assíncronos, e integração flexível para automação de testes. O projeto inclui um adaptador de testes para integração com ferramentas como Visual Studio Test Explorer e `dotnet test`.

## O que é BDD e por que é importante?

**BDD (Behavior Driven Development)** é uma abordagem de desenvolvimento de software que incentiva a colaboração entre desenvolvedores, QA e stakeholders não técnicos. O foco está em descrever o comportamento do sistema em linguagem natural, facilitando o entendimento e a validação dos requisitos.

### Benefícios do BDD:
- **Comunicação clara:** aproxima desenvolvedores e área de negócio, reduzindo ambiguidades.
- **Documentação viva:** os cenários servem como documentação executável do sistema.
- **Testes orientados ao comportamento:** garante que o software atenda às expectativas do usuário final.
- **Facilidade de manutenção:** cenários bem definidos facilitam a evolução e refatoração do código.

## Estrutura do Projeto

```csharp
MVFC.StoryTest.sln

MVFC.StoryTest # Framework principal (core BDD)

MVFC.StoryTest.TestAdapter # Adaptador para integração com runners de teste .NET

MVFC.StoryTest.Tests # Exemplos e testes de uso do framework

```

## Recursos

- Definição fluente de cenários de teste (Given/When/Then)
- Suporte a steps síncronos e assíncronos
- Hooks para execução antes/depois de cenários e steps
- Contexto de cenário extensível
- Saída colorida no console para fácil visualização dos resultados
- Integração simples com projetos .NET 9+
- Adaptador para execução dos cenários como testes convencionais (`dotnet test`, Visual Studio, Azure DevOps, etc.)

## Instalação

Adicione a referência ao pacote principal no seu projeto .NET 9:

```sh
dotnet add package MVFC.StoryTest.TestAdapter
```

## Exemplos de Uso

### 1 - Por método (dentro de uma classe de teste tradicional)

* Como funciona:

    - Você define cenários diretamente em métodos de teste, usando a API fluente do framework.

* Principais Atributos/métodos necessários:

  - **[Fact] (ou [TestMethod]/[Test])**: atributo do framework de teste (xUnit, MSTest, NUnit).
  
  - **Story.Scenario<T>()**: método estático para iniciar o cenário, onde T é o tipo do contexto.
  
  - **.WithContext():** inicializa o contexto do cenário.
  
  - **.BeforeScenario(...):** define ações a serem executadas antes do cenário.
  
  - **.Given(...), .When(...), .Then(...)**: definem os steps do cenário.
  
  - **.Run()**: executa o cenário.

Exemplo mínimo:

```csharp
namespace MVFC.StoryTest.Tests;

public sealed class AccountTests
{
    [Fact]
    public void Executa_Sincrono_CaminhoFeliz_NaoLancaExcecao()
    {
        Story.Scenario<AccountSimple>("Caminho feliz síncrono")
            .WithContext()
            .BeforeScenario(acc => acc.Deposit(100))
            .Given("sacar 10", acc => acc.Withdraw(10))
            .Then("saldo deve ser 90", acc => acc.Balance.Should().Be(90))
            .Run();
    }
    ...
}
```

### 2 - Por classe (cada cenário é uma classe)

* Como funciona:
  
    - Cada cenário é uma classe, com métodos marcados por atributos que representam os steps.

* Principais Atributos/métodos necessários:

    - **[Scenario]**: atributo para marcar a classe como cenário, com descrição, feature e tags opcionais.
    
    - **[BeforeScenario], [AfterScenario]**: métodos executados antes/depois do cenário.
    
    - **[Given], [When], [Then]**: métodos que representam steps, podem ter ordem (Order).
    
    - Propriedades para armazenar contexto e dados do cenário.

    - **Context**: propriedade para acessar o contexto compartilhado do cenário.

Exemplo mínimo:

```csharp
namespace MVFC.StoryTest.Tests;

[Scenario("Realizar transferência bancária entre contas com validações complexas",
          Feature = "Sistema Bancário",
          Tags = new[] { "banking", "transfer", "complex" })]
public sealed class TransferenciaBancariaScenario
{
    private Account _contaOrigem = default!;
    private Account _contaDestino = default!;

    public ScenarioContextAttribute Context { get; set; } = default!;

    [BeforeScenario]
    public void Setup()
    {
        Context.Set("saldo_inicial_origem", 10000m);
        Context.Set("saldo_inicial_destino", 5000m);
    }

    [Given("Que tenho uma conta origem com saldo de R$ 10.000", Order = 1)]
    public void DadoContaOrigem()
    {
        _contaOrigem = new Account("0001-X", "João Silva", 10000m);

        _contaOrigem.Should().NotBeNull();
        _contaOrigem.Balance.Should().Be(10000m);
        _contaOrigem.Status.Should().Be(AccountStatus.Active);
        _contaOrigem.Transactions.Should().BeEmpty("conta nova não tem transações");
    }

    [Given("E tenho uma conta destino com saldo de R$ 5.000", Order = 2)]
    public void DadoContaDestino()
    {
        _contaDestino = new Account("0002-Y", "Maria Santos", 5000m);

        _contaDestino.Should().NotBeNull();
        _contaDestino.Balance.Should().Be(5000m);
    }

    [Given("E o limite diário de transferência é R$ 5.000", Order = 3)]
    public void DadoLimiteDiario()
    {
        _contaOrigem.DailyLimit.Should().Be(5000m);
    }

    [When("Eu transfiro R$ 3.500 da conta origem para a conta destino")]
    public void QuandoRealizoTransferencia()
    {
        _contaOrigem.Transfer(_contaDestino, 3500m, "Pagamento fornecedor");

        Context.Set("valor_transferido", 3500m);
        Context.Set("timestamp_transferencia", DateTime.Now);
    }

    [Then("A conta origem deve ter saldo de R$ 6.500", Order = 1)]
    public void EntaoContaOrigemDeveTerNovoSaldo()
    {
        var saldoEsperado = Context.Get<decimal>("saldo_inicial_origem") -
                           Context.Get<decimal>("valor_transferido");

        _contaOrigem.Balance.Should().Be(6500m)
            .And.Be(saldoEsperado);
    }

    [Then("A conta destino deve ter saldo de R$ 8.500", Order = 2)]
    public void EntaoContaDestinoDeveTerNovoSaldo()
    {
        _contaDestino.Balance.Should().Be(8500m);
    }

    [Then("A conta origem deve ter 1 transação de débito", Order = 3)]
    public void EntaoContaOrigemDeveTerTransacaoDebito()
    {
        _contaOrigem.Transactions.Should()
            .HaveCount(1)
            .And.ContainSingle(t => t.Type == TransactionType.Debit)
            .Which.Should().Match<Transaction>(t =>
                t.Amount == 3500m &&
                t.Description.Contains("Pagamento fornecedor") &&
                t.Description.Contains("0002-Y") &&
                t.BalanceAfter == 6500m);
    }

    [Then("A conta destino deve ter 1 transação de crédito", Order = 4)]
    public void EntaoContaDestinoDeveTerTransacaoCredito()
    {
        _contaDestino.Transactions.Should()
            .HaveCount(1)
            .And.ContainSingle(t => t.Type == TransactionType.Credit)
            .Which.Should().Match<Transaction>(t =>
                t.Amount == 3500m &&
                t.Description.Contains("Pagamento fornecedor") &&
                t.Description.Contains("0001-X") &&
                t.BalanceAfter == 8500m);
    }

    [Then("Ambas as transações devem ter timestamp recente", Order = 5)]
    public void EntaoTransacoesDevemTerTimestamp()
    {
        var timestampTransferencia = Context.Get<DateTime>("timestamp_transferencia");

        _contaOrigem.Transactions[0].Date.Should()
            .BeCloseTo(timestampTransferencia, TimeSpan.FromSeconds(1));

        _contaDestino.Transactions[0].Date.Should()
            .BeCloseTo(timestampTransferencia, TimeSpan.FromSeconds(1));
    }

    [AfterScenario]
    public void Cleanup()
    {
        var resumo = new
        {
            ContaOrigemFinal = _contaOrigem.Balance,
            ContaDestinoFinal = _contaDestino.Balance,
            TotalTransacoes = _contaOrigem.Transactions.Count + _contaDestino.Transactions.Count
        };

        Context.Set("resumo_final", resumo);
    }
}
```

### 3 - Por classe + Arquivo feature

* Como funciona:
    - Você descreve cenários em arquivos .feature (Gherkin) e implementa os steps em classes C#.

* Principais Atributos/métodos necessários:
    - Arquivo .feature descrevendo **Feature, Scenario, Given/When/Then**.
    
    - Classe base com métodos marcados por **[Given], [When], [Then], [BeforeScenario], [AfterScenario]**.
    
    - **[StoryBinding("Feature", "Scenario")]**: atributo que vincula a classe ao cenário do arquivo feature.
    
    - Métodos de step podem usar expressões regulares para capturar parâmetros do texto do step.
    
    - Propriedades para armazenar dados compartilhados entre os steps.

Exemplo mínimo:

```feature
@api @products
Feature: Product Registration

Scenario: Create a new product successfully
  Given I provide the data for a new product with name "Test Product" and price 99.99
  When I send a request to create the product
  Then the product should be created successfully
  And I should be able to retrieve this product by its ID
```

### Classe base com métodos comuns para todos os cenários

```csharp
namespace MVFC.StoryTest.Tests.Products;

public abstract class ProductsSteps
{
    protected ProdutoService? _service;
    protected CriarProdutoRequest? _request;
    protected Produto? _createdProduct;
    protected Exception? _exception;

    [BeforeScenario]
    public void Setup()
    {
        _service = new ProdutoService();
    }

    [Given("I provide the data for a new product with name \"(.*)\" and price ([\\d.-]+(?:\\.[\\d]+)?)")]
    public void GivenIProvideTheDataForANewProduct(string name, decimal price)
    {
        _request = new CriarProdutoRequest(name, price);
    }

    [When("I send a request to create the product")]
    public void WhenISendARequestToCreateTheProduct()
    {
        try
        {
            _createdProduct = _service?.CriarProduto(_request!);
            _exception = null;
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [AfterScenario]
    public void Cleanup()
    {
        _service?.LimparProdutos();
    }
}
```
### Classe com o cenário específicos

```csharp
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
```