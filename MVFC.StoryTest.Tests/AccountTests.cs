namespace MVFC.StoryTest.Tests;

public sealed class AccountTests {
    [Fact]
    public void Executa_Sincrono_CaminhoFeliz_NaoLancaExcecao() =>
        Story.Scenario<AccountSimple>("Caminho feliz síncrono")
            .WithContext()
            .BeforeScenario(acc => acc.Deposit(100))
            .Given("sacar 10", acc => acc.Withdraw(10))
            .Then("saldo deve ser 90", acc => acc.Balance.Should().Be(90))
            .Run();

    [Fact]
    public void Executa_Sincrono_PassoLancaExcecao_PropagaExcecao() {
        var story = Story.Scenario<AccountSimple>("Passo síncrono lança exceção")
            .WithContext()
            .Given("passo lança exceção", _ => throw new InvalidOperationException("boom"));

        var ex = Assert.Throws<InvalidOperationException>(() => story.Run());
        Assert.Contains("boom", ex.Message);
    }

    [Fact]
    public async Task Executa_Assincrono_CaminhoFeliz_NaoLancaExcecao() =>
        await Story.Scenario<AccountSimple>("Caminho feliz assíncrono")
            .WithContext()
            .BeforeScenario(async acc => {
                await Task.Delay(1);
                acc.Deposit(50);
            })
            .Given("depósito assíncrono", async acc => {
                await Task.Delay(1);
                acc.Deposit(50);
            })
            .When("saque assíncrono", async acc => {
                await Task.Delay(1);
                acc.Withdraw(20);
            })
            .Then("verifica saldo", async acc => {
                await Task.Delay(1);
                acc.Balance.Should().Be(80);
            })
            .RunAsync();

    [Fact]
    public void Executa_Sincrono_SuportaPassosAssincronos() =>
        Story.Scenario<AccountSimple>("Executa síncrono com passo assíncrono")
            .WithContext()
            .Given("depósito assíncrono", async acc => {
                await Task.Delay(1);
                acc.Deposit(30);
            })
            .Then("saldo deve ser 30", acc => acc.Balance.Should().Be(30))
            .Run();

    [Fact]
    public async Task Executa_Assincrono_SuportaPassosSincronos() =>
        await Story.Scenario<AccountSimple>("Executa assíncrono com passos síncronos")
            .WithContext()
            .Given("depósito síncrono", acc => acc.Deposit(15))
            .When("saque síncrono", acc => acc.Withdraw(5))
            .Then("verifica saldo assíncrono", async acc => {
                await Task.Delay(1);
                acc.Balance.Should().Be(10);
            })
            .RunAsync();

    [Fact]
    public void Executa_Sincrono_HooksChamadosNaOrdem() {
        var hist = new HookHistory();
        Story.Scenario<HookHistory>("Hooks síncronos em ordem")
            .WithContext(hist)
            .BeforeScenario(h => h.Events.Add("AntesDoCenario"))
            .BeforeStep(h => h.Events.Add("AntesDoPasso"))
            .Given("executa passo", h => h.Events.Add("Passo"))
            .AfterStep(h => h.Events.Add("DepoisDoPasso"))
            .AfterScenario(h => h.Events.Add("DepoisDoCenario"))
            .Run();

        Assert.Equal(["AntesDoCenario", "AntesDoPasso", "Passo", "DepoisDoPasso", "DepoisDoCenario"], hist.Events);
    }

    [Fact]
    public async Task Executa_Assincrono_HooksAssincronosChamados() {
        var hist = new HookHistory();
        await Story.Scenario<HookHistory>("Hooks assíncronos em ordem")
            .WithContext(hist)
            .BeforeScenario(async h => { await Task.Delay(1); h.Events.Add("AntesDoCenarioAsync"); })
            .BeforeStep(async h => { await Task.Delay(1); h.Events.Add("AntesDoPassoAsync"); })
            .Given("executa passo assíncrono", async h => { await Task.Delay(1); h.Events.Add("PassoAsync"); })
            .AfterStep(async h => { await Task.Delay(1); h.Events.Add("DepoisDoPassoAsync"); })
            .AfterScenario(async h => { await Task.Delay(1); h.Events.Add("DepoisDoCenarioAsync"); })
            .RunAsync();

        Assert.Equal(["AntesDoCenarioAsync", "AntesDoPassoAsync", "PassoAsync", "DepoisDoPassoAsync", "DepoisDoCenarioAsync"], hist.Events);
    }

    [Fact]
    public void ComContexto_InstanciaFornecida_EUtilizada() {
        var acc = new AccountSimple();
        Story.Scenario<AccountSimple>("Usa instância fornecida")
            .WithContext(acc)
            .Given("depositar 5", a => a.Deposit(5))
            .Then("saldo deve ser 5", a => a.Balance.Should().Be(5))
            .Run();

        Assert.Equal(5, acc.Balance);
    }

    [Fact]
    public void Executa_Sincrono_InvocaHooksAssincronos() {
        var hist = new HookHistory();
        Story.Scenario<HookHistory>("Executa síncrono invocando hooks assíncronos")
            .WithContext(hist)
            .BeforeScenario(async h => { await Task.Delay(1); h.Events.Add("AntesDoCenarioAsync"); })
            .Given("não faz nada", h => h.Events.Add("Passo"))
            .AfterScenario(async h => { await Task.Delay(1); h.Events.Add("DepoisDoCenarioAsync"); })
            .Run();

        Assert.Equal(["AntesDoCenarioAsync", "Passo", "DepoisDoCenarioAsync"], hist.Events);
    }

    [Fact]
    public async Task Executa_Assincrono_PropagaExcecaoDePassoSincrono() {
        var scenario = Story.Scenario<AccountSimple>("Executa assíncrono com passo síncrono que lança exceção")
            .WithContext()
            .Given("passo síncrono lança exceção", _ => throw new ArgumentOutOfRangeException("x"));

        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await scenario.RunAsync());
        Assert.Equal("x", ex.ParamName);
    }

    [Fact]
    public void Executa_MultiplosPassos_MantemEstadoEntrePassos() =>
        Story.Scenario<AccountSimple>("Propagação de estado entre múltiplos passos")
            .WithContext()
            .Given("depositar 10", acc => acc.Deposit(10))
            .When("depositar 20", acc => acc.Deposit(20))
            .Then("saldo deve ser 30", acc => acc.Balance.Should().Be(30))
            .Run();
}