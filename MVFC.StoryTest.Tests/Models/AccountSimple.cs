namespace MVFC.StoryTest.Tests.Models;

public sealed class AccountSimple
{
    public int Balance { get; private set; }

    public void Deposit(int value) => Balance += value;

    public void Withdraw(int value)
    {
        if (Balance < value)
            throw new InvalidOperationException("Insufficient");
        Balance -= value;
    }
}