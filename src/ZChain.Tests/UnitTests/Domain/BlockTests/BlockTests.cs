using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core;
using ZChain.Core.Builder;

namespace ZChain.Tests.UnitTests.Domain.BlockTests;

public class BlockTests
{
    private readonly Block<MoneyTransferTransaction> _rootBlock;
    private readonly MoneyTransferTransaction _moneyTransferDummyTransaction = new TransactionBuilder()
        .WithFromAddress("Second_Address")
        .WithToAddress("Third_Address")
        .WithAmount(200)
        .Build();
    private readonly IMiner<MoneyTransferTransaction> _miner = new StubMiner<MoneyTransferTransaction>("789822be49ab427da24a6477dbb4f24e", "0000BCA37B7928378F46215DB844BF8D61047E31D417731B2A8D0D9872138BDF");

    class StubMiner<T> : IMiner<T>
    {
        private readonly string _nonce;
        private readonly string _hash;

        public StubMiner(string nonce, string hash)
        {
            _nonce = nonce;
            _hash = hash;
        }

        public Task MineBlock(Block<T> blockToMine)
        {
            blockToMine.SetMiningBeginning();
            blockToMine.SetMinedValues(_nonce, _hash);
            return Task.CompletedTask;
        }
    }

    public BlockTests()
    {
        var rootTransaction = new TransactionBuilder()
            .WithFromAddress("First_Address")
            .WithToAddress("Second_Address")
            .WithAmount(300)
            .Build();
        _rootBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(null)
            .WithTransaction(rootTransaction)
            .WithDifficulty(4)
            .Build();
        _miner.MineBlock(_rootBlock);
    }

    [Fact]
    public void WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow()
    {
        Should.Throw<InvalidOperationException>(() => _miner.MineBlock(_rootBlock));
    }

    [Fact]
    public void WhenMiningABlock_AndSettingABadNonce_ShouldThrow()
    {
        var firstChild = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(_rootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(4)
            .Build();
        Should.Throw<BlockStateException>(() => _miner.MineBlock(firstChild));
    }

    [Fact]
    public void WhenHavingANewBlock_AndAttemptingToVerify_ShouldThrow()
    {
        var newBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(_rootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(2)
            .Build();
        Should.Throw<BlockStateException>(() => newBlock.VerifyMinedBlock());
    }

    [Fact]
    public void WhenMiningABlockAndSettingValues_AndTheHashDoesntMatchTheExpectedDifficulty_Throws()
    {
        var rootBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(null)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(2)
            .Build();
        var newBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(rootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(2)
            .Build();
        Should.Throw<BlockStateException>(() => _miner.MineBlock(newBlock));
    }
}
