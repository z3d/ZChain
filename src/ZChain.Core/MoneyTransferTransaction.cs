namespace ZChain.Core;

public class MoneyTransferTransaction
{
    public MoneyTransferTransaction(string fromAddress, string toAddress, decimal amount)
    {
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Amount = amount;
    }
    public override string ToString()
    {
        return $"From {FromAddress}, To {ToAddress}, Amount {Amount}";
    }
    public string FromAddress { get; private set; }
    public string ToAddress { get; private set; }
    public decimal Amount { get; private set; }
}