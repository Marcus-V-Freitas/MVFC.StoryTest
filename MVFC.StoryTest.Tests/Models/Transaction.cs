namespace MVFC.StoryTest.Tests.Models;

public sealed class Transaction
{
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = default!;
    public DateTime Date { get; set; }
    public decimal BalanceAfter { get; set; }
}