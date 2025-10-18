namespace MVFC.StoryTest.Tests.Models;

public sealed class Account(string accountNumber, string owner, decimal initialBalance = 0)
{
    public string AccountNumber { get; } = accountNumber;

    public string Owner { get; } = owner;

    public decimal Balance { get; private set; } = initialBalance;

    public string? Reason { get; private set; }

    public AccountStatus Status { get; private set; } = AccountStatus.Active;

    public decimal DailyLimit { get; set; } = 5000m;

    private readonly List<Transaction> _transactions = [];

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    public void Deposit(decimal amount, string description = "Depósito")
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Conta inativa não pode receber depósitos");

        if (amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");

        Balance += amount;
        _transactions.Add(new Transaction
        {
            Type = TransactionType.Credit,
            Amount = amount,
            Description = description,
            Date = DateTime.Now,
            BalanceAfter = Balance
        });
    }

    public void Withdraw(decimal amount, string description = "Saque")
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Conta inativa");

        if (amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");

        var todayWithdrawals = _transactions
            .Where(t => t.Type == TransactionType.Debit && t.Date.Date == DateTime.Today)
            .Sum(t => t.Amount);

        if (todayWithdrawals + amount > DailyLimit)
            throw new InvalidOperationException($"Limite diário excedido. Disponível: R$ {DailyLimit - todayWithdrawals:F2}");

        if (amount > Balance)
            throw new InvalidOperationException($"Saldo insuficiente. Disponível: R$ {Balance:F2}");

        Balance -= amount;
        _transactions.Add(new Transaction
        {
            Type = TransactionType.Debit,
            Amount = amount,
            Description = description,
            Date = DateTime.Now,
            BalanceAfter = Balance
        });
    }

    public void Transfer(Account destinationAccount, decimal amount, string description = "Transferência")
    {
        Withdraw(amount, $"{description} para {destinationAccount.AccountNumber}");
        destinationAccount.Deposit(amount, $"{description} de {AccountNumber}");
    }

    public void Block(string reason)
    {
        Status = AccountStatus.Blocked;
        Reason = reason;
    }

    public decimal GetMonthlyBalance(int month, int year) =>
        _transactions
            .Where(t => t.Date.Month == month && t.Date.Year == year)
            .Sum(t => t.Type == TransactionType.Credit ? t.Amount : -t.Amount);
}