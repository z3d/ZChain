namespace ZChain.Core;

public class MoneyTransferDummyTransaction
{
    public MoneyTransferDummyTransaction(string fromAddress, string toAddress, int amount)
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
    public int Amount { get; private set; }
}