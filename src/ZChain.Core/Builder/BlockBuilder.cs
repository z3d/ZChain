namespace ZChain.Core.Builder;

public class BlockBuilder<T>
{
    private Block<T> _previousBlock;
    private T _transaction;
    private int _difficulty;

    public BlockBuilder<T> WithPreviousBlock(Block<T> previousBlock)
    {
        _previousBlock = previousBlock;
        return this;
    }

    public BlockBuilder<T> WithTransaction(T transaction)
    {
        _transaction = transaction;
        return this;
    }

    public BlockBuilder<T> WithDifficulty(int difficulty)
    {
        _difficulty = difficulty;
        return this;
    }

    public Block<T> Build()
    {
        return new Block<T>(_previousBlock, _transaction, _difficulty);
    }
}
