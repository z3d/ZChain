namespace ZChain.Core;

public class MoneyTransferTransaction(string fromAddress, string toAddress, decimal amount)
{
    public override string ToString()
    {
        return $"From {FromAddress}, To {ToAddress}, Amount {Amount}";
    }
    public string FromAddress { get; private set; } = fromAddress;
    public string ToAddress { get; private set; } = toAddress;
    public decimal Amount { get; private set; } = amount;
}
