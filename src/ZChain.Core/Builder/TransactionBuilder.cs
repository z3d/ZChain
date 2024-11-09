namespace ZChain.Core.Builder;

public class TransactionBuilder
{
    private string _fromAddress;
    private string _toAddress;
    private decimal _amount;

    public TransactionBuilder WithFromAddress(string fromAddress)
    {
        _fromAddress = fromAddress;
        return this;
    }

    public TransactionBuilder WithToAddress(string toAddress)
    {
        _toAddress = toAddress;
        return this;
    }

    public TransactionBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public MoneyTransferTransaction Build()
    {
        return new MoneyTransferTransaction(_fromAddress, _toAddress, _amount);
    }
}
