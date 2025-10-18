namespace MVFC.StoryTest.Tests;

[Scenario("Realizar transferência bancária entre contas com validações complexas",
          Feature = "Sistema Bancário",
          Tags = new[] { "banking", "transfer", "complex" })]
public sealed class TransferenciaBancariaScenario {
    private Account _contaOrigem = default!;
    private Account _contaDestino = default!;

    public ScenarioContextAttribute Context { get; set; } = default!;

    [BeforeScenario]
    public void Setup() {
        Context.Set("saldo_inicial_origem", 10000m);
        Context.Set("saldo_inicial_destino", 5000m);
    }

    [Given("Que tenho uma conta origem com saldo de R$ 10.000", Order = 1)]
    public void DadoContaOrigem() {
        _contaOrigem = new Account("0001-X", "João Silva", 10000m);

        _contaOrigem.Should().NotBeNull();
        _contaOrigem.Balance.Should().Be(10000m);
        _contaOrigem.Status.Should().Be(AccountStatus.Active);
        _contaOrigem.Transactions.Should().BeEmpty("conta nova não tem transações");
    }

    [Given("E tenho uma conta destino com saldo de R$ 5.000", Order = 2)]
    public void DadoContaDestino() {
        _contaDestino = new Account("0002-Y", "Maria Santos", 5000m);

        _contaDestino.Should().NotBeNull();
        _contaDestino.Balance.Should().Be(5000m);
    }

    [Given("E o limite diário de transferência é R$ 5.000", Order = 3)]
    public void DadoLimiteDiario() =>
        _contaOrigem.DailyLimit.Should().Be(5000m);

    [When("Eu transfiro R$ 3.500 da conta origem para a conta destino")]
    public void QuandoRealizoTransferencia() {
        _contaOrigem.Transfer(_contaDestino, 3500m, "Pagamento fornecedor");

        Context.Set("valor_transferido", 3500m);
        Context.Set("timestamp_transferencia", DateTime.Now);
    }

    [Then("A conta origem deve ter saldo de R$ 6.500", Order = 1)]
    public void EntaoContaOrigemDeveTerNovoSaldo() {
        var saldoEsperado = Context.Get<decimal>("saldo_inicial_origem") -
                           Context.Get<decimal>("valor_transferido");

        _contaOrigem.Balance.Should().Be(6500m)
            .And.Be(saldoEsperado);
    }

    [Then("A conta destino deve ter saldo de R$ 8.500", Order = 2)]
    public void EntaoContaDestinoDeveTerNovoSaldo() =>
        _contaDestino.Balance.Should().Be(8500m);

    [Then("A conta origem deve ter 1 transação de débito", Order = 3)]
    public void EntaoContaOrigemDeveTerTransacaoDebito() =>
        _contaOrigem.Transactions.Should()
            .HaveCount(1)
            .And.ContainSingle(t => t.Type == TransactionType.Debit)
            .Which.Should().Match<Transaction>(t =>
                t.Amount == 3500m &&
                t.Description.Contains("Pagamento fornecedor") &&
                t.Description.Contains("0002-Y") &&
                t.BalanceAfter == 6500m);

    [Then("A conta destino deve ter 1 transação de crédito", Order = 4)]
    public void EntaoContaDestinoDeveTerTransacaoCredito() =>
        _contaDestino.Transactions.Should()
            .HaveCount(1)
            .And.ContainSingle(t => t.Type == TransactionType.Credit)
            .Which.Should().Match<Transaction>(t =>
                t.Amount == 3500m &&
                t.Description.Contains("Pagamento fornecedor") &&
                t.Description.Contains("0001-X") &&
                t.BalanceAfter == 8500m);

    [Then("Ambas as transações devem ter timestamp recente", Order = 5)]
    public void EntaoTransacoesDevemTerTimestamp() {
        var timestampTransferencia = Context.Get<DateTime>("timestamp_transferencia");

        _contaOrigem.Transactions[0].Date.Should()
            .BeCloseTo(timestampTransferencia, TimeSpan.FromSeconds(1));

        _contaDestino.Transactions[0].Date.Should()
            .BeCloseTo(timestampTransferencia, TimeSpan.FromSeconds(1));
    }

    [AfterScenario]
    public void Cleanup() {
        var resumo = new {
            ContaOrigemFinal = _contaOrigem.Balance,
            ContaDestinoFinal = _contaDestino.Balance,
            TotalTransacoes = _contaOrigem.Transactions.Count + _contaDestino.Transactions.Count
        };

        Context.Set("resumo_final", resumo);
    }
}

[Scenario("Tentar transferência acima do limite diário deve falhar",
          Feature = "Sistema Bancário",
          Tags = new[] { "banking", "validation", "limite" })]
public class TransferenciaAcimaLimiteScenario {
    private Account _contaOrigem = default!;
    private Account _contaDestino = default!;
    private Action _acao = default!;

    [Given("Que tenho uma conta com saldo de R$ 10.000 e limite diário de R$ 5.000", Order = 1)]
    public void DadoContaComLimite() =>
        _contaOrigem = new Account("0001-X", "João Silva", 10000m) {
            DailyLimit = 5000m
        };

    [Given("E já realizei um saque de R$ 2.000 hoje", Order = 2)]
    public void DadoSaqueAnterior() {
        _contaOrigem!.Withdraw(2000m, "Saque ATM");

        _contaOrigem.Transactions.Should().HaveCount(1);
        _contaOrigem.Balance.Should().Be(8000m);
    }

    [Given("E tenho uma conta destino", Order = 3)]
    public void DadoContaDestino() =>
        _contaDestino = new Account("0002-Y", "Maria Santos", 1000m);

    [When("Eu tento transferir R$ 4.000 (excedendo o limite restante de R$ 3.000)")]
    public void QuandoTentoTransferirAcimaDoLimite() =>
        _acao = () => _contaOrigem?.Transfer(_contaDestino, 4000m);

    [Then("Deve lançar InvalidOperationException com mensagem sobre limite diário")]
    public void EntaoDeveLancarExcecaoLimite() =>
        _acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*Limite diário excedido*")
            .And.Message.Should().Contain("Disponível: R$ 3000");

    [Then("O saldo da conta origem deve permanecer R$ 8.000")]
    public void EntaoSaldoOrigemDevePermanecer() =>
        _contaOrigem?.Balance.Should().Be(8000m);

    [Then("O saldo da conta destino deve permanecer R$ 1.000")]
    public void EntaoSaldoDestinoDevePermanecer() =>
        _contaDestino?.Balance.Should().Be(1000m);

    [Then("Nenhuma nova transação deve ser registrada")]
    public void EntaoNenhumaNovaTransacao() {
        _contaOrigem?.Transactions.Should().HaveCount(1, "apenas o saque anterior");
        _contaDestino?.Transactions.Should().BeEmpty();
    }
}

[Scenario("Operações em conta bloqueada devem ser rejeitadas",
          Feature = "Sistema Bancário",
          Tags = new[] { "banking", "security", "blocked" })]
public class OperacoesContaBloqueadaScenario {
    private Account _conta = default!;
    private Action _acaoDeposito = default!;

    [Given("Que tenho uma conta ativa com saldo de R$ 1.000", Order = 1)]
    public void DadoContaAtiva() {
        _conta = new Account("0001-X", "João Silva", 1000m);
        _conta.Status.Should().Be(AccountStatus.Active);
    }

    [Given("E a conta foi bloqueada por suspeita de fraude", Order = 2)]
    public void DadoContaBloqueada() {
        _conta.Block("Suspeita de fraude detectada");

        _conta.Status.Should().Be(AccountStatus.Blocked);
    }

    [When("Eu tento fazer um depósito de R$ 500")]
    public void QuandoTentoDeposito() =>
        _acaoDeposito = () => _conta.Deposit(500m);

    [Then("O depósito deve ser rejeitado")]
    public void EntaoDepositoDeveSerRejeitado() =>
        _acaoDeposito.Should().Throw<InvalidOperationException>()
            .WithMessage("*inativa*");

    [Then("O saldo deve permanecer R$ 1.000")]
    public void EntaoSaldoDevePermanecer() =>
        _conta.Balance.Should().Be(1000m);

    [Then("Nenhuma transação deve ser registrada")]
    public void EntaoNenhumaTransacao() =>
        _conta.Transactions.Should().BeEmpty();
}
