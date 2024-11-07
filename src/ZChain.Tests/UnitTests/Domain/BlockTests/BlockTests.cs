using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core;
using ZChain.Core.Builder;
using ZChain.Hashers;

namespace ZChain.Tests.UnitTests.Domain.BlockTests;

public class BlockTests
{
    private const string TestNonce = "TEST_NONCE";
    private const string TestHash = "0000TEST_HASH";
    private const string FromAddress1 = "First_Address";
    private const string FromAddress2 = "Second_Address";
    private const string ToAddress1 = "Second_Address";
    private const string ToAddress2 = "Third_Address";
    private const decimal Amount1 = 300;
    private const decimal Amount2 = 200;

    private readonly Block<MoneyTransferTransaction> _minedRootBlock;
    private readonly MoneyTransferTransaction _moneyTransferDummyTransaction = new TransactionBuilder()
        .WithFromAddress(FromAddress2)
        .WithToAddress(ToAddress2)
        .WithAmount(Amount2)
        .Build();
    private readonly IMiner<MoneyTransferTransaction> _miner = new StubMiner<MoneyTransferTransaction>(TestNonce, TestHash);
    private readonly IHasher _hasher = new StubHasher();

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

    class StubHasher : IHasher
    {
        public string ComputeHash(string input)
        {
            // Return a predictable hash value for testing purposes
            return TestHash;
        }
    }

    public BlockTests()
    {
        var rootTransaction = new TransactionBuilder()
            .WithFromAddress(FromAddress1)
            .WithToAddress(ToAddress1)
            .WithAmount(Amount1)
            .Build();
        _minedRootBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(null)
            .WithTransaction(rootTransaction)
            .WithDifficulty(4)
            .WithHasher(_hasher)
            .Build();
        _miner.MineBlock(_minedRootBlock);
    }

    [Fact]
    public void WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow()
    {
        Should.Throw<InvalidOperationException>(() => _miner.MineBlock(_minedRootBlock));
    }  

    [Fact]
    public void WhenHavingANewBlock_AndAttemptingToVerify_ShouldThrow()
    {
        var newBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(_minedRootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(2)
            .WithHasher(_hasher)
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
            .WithHasher(_hasher)
            .Build();
        var newBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(rootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(2)
            .WithHasher(_hasher)
            .Build();
        Should.Throw<BlockStateException>(() => _miner.MineBlock(newBlock));
    }

    [Fact]
    public void WhenCreatingANewBlock_ItShouldHaveCorrectInitialState()
    {
        var newBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(_minedRootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(3)
            .WithHasher(_hasher)
            .Build();
        newBlock.State.ShouldBe(BlockState.New);
        newBlock.Height.ShouldBe(_minedRootBlock.Height + 1);
        newBlock.Parent.ShouldBe(_minedRootBlock);
    }

    [Fact]
    public void WhenVerifyingANonMinedBlock_ItShouldThrow()
    {
        Should.Throw<BlockStateException>(() =>
        {
            var newBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(_minedRootBlock)
            .WithTransaction(_moneyTransferDummyTransaction)
            .WithDifficulty(3)
            .WithHasher(_hasher)
            .Build();
            newBlock.VerifyMinedBlock().ShouldBeFalse();
        });
    }

    [Fact]
    public void WhenCreatingABlockWithInvalidTransaction_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() =>
        {
            new BlockBuilder<MoneyTransferTransaction>()
                .WithPreviousBlock(_minedRootBlock)
                .WithTransaction(null)
                .WithDifficulty(3)
                .WithHasher(_hasher)
                .Build();
        });
    }
}
