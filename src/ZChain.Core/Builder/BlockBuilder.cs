using System;

namespace ZChain.Core.Builder;

public class BlockBuilder<T>
{
    private Block<T> _previousBlock;
    private T _transaction;
    private int _difficulty;
    private IHasher _hasher;

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

    public BlockBuilder<T> WithHasher(IHasher hasher)
    {
        _hasher = hasher;
        return this;
    }

    public Block<T> Build()
    {
        if (_hasher == null)
        {
            throw new InvalidOperationException("Hasher must be provided");
        }

        return new Block<T>(_previousBlock, _transaction, _difficulty, _hasher);
    }
}
