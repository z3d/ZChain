using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core;
using ZChain.Tests.Builder;

namespace ZChain.Tests.UnitTests.Domain.BlockTests
{
    public class BlockTests
    {
        private readonly Block<MoneyTransferDummyTransaction> _rootBlock;
        private readonly MoneyTransferDummyTransaction _moneyTransferDummyTransaction = new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200);
        private readonly IMiner<MoneyTransferDummyTransaction> _miner = new StubMiner<MoneyTransferDummyTransaction>("c349c83bd38c46c48321c7f9fbcffe3a", "0000BF5EBA588845258F54C0E3D07EE2CCA2E3F01135225D291BF0717353AA67");

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
            _rootBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(null)
                .WithTransaction(new MoneyTransferDummyTransaction("First_Address", "Second_Address", 300))
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
            var firstChild = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(_rootBlock)
                .WithTransaction(_moneyTransferDummyTransaction)
                .WithDifficulty(4)
                .Build();
            Should.Throw<BlockStateException>(() => _miner.MineBlock(firstChild));
        }

        [Fact]
        public void WhenHavingANewBlock_AndAttemptingToVerify_ShouldThrow()
        {
            var newBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(_rootBlock)
                .WithTransaction(_moneyTransferDummyTransaction)
                .WithDifficulty(2)
                .Build();
            Should.Throw<BlockStateException>(() => newBlock.VerifyMinedBlock());
        }

        [Fact]
        public void WhenMiningABlockAndSettingValues_AndTheHashDoesntMatchTheExpectedDifficulty_Throws()
        {
            var rootBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(null)
                .WithTransaction(_moneyTransferDummyTransaction)
                .WithDifficulty(2)
                .Build();
            var newBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(rootBlock)
                .WithTransaction(_moneyTransferDummyTransaction)
                .WithDifficulty(2)
                .Build();
            Should.Throw<BlockStateException>(() => _miner.MineBlock(newBlock));
        }
    }
}
