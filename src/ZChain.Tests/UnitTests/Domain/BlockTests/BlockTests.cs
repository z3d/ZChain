using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core.Tree;

namespace ZChain.Tests.UnitTests.Domain.BlockTests
{
    public class BlockTests
    {
        private readonly Block<MoneyTransferDummyTransaction> _firstChild;
        private readonly MoneyTransferDummyTransaction _moneyTransferDummyTransaction = new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200);

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
            var rootBlock = Block<MoneyTransferDummyTransaction>.CreateGenesisBlock(new MoneyTransferDummyTransaction("x", "y", 1));
            _firstChild = new Block<MoneyTransferDummyTransaction>(rootBlock, _moneyTransferDummyTransaction, 2);
        }

        [Fact]
        public void WhenConstructingABlock_AndTheParentBlockIsNull_ShouldThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Should.Throw<ArgumentNullException>(() => new Block<MoneyTransferDummyTransaction>(null, _moneyTransferDummyTransaction, 1));
        }

        [Fact]
        public void WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow()
        {
            var miner = new StubMiner<MoneyTransferDummyTransaction>("3ae359e12e9a4ad19559fbffe8f3c15a", "0041AD4CC3B9C508336BA949D302076E0A47DAB769B7D2A6F216791A913A4036");
            miner.MineBlock(_firstChild);

            Should.Throw<InvalidOperationException>(() => miner.MineBlock(_firstChild));
        }

        [Fact]
        public void WhenMiningABlock_AndSettingABadNonce_ShouldThrow()
        {
            var miner = new StubMiner<MoneyTransferDummyTransaction>("31e359e12e9a4ad19559fbffe8f3c15a", "0041AD4CC3B9C508336BA949D302076E0A47DAB769B7D2A6F216791A913A4036");
            Should.Throw<InvalidOperationException>(() => miner.MineBlock(_firstChild));
        }

        [Fact]
        public void WhenHavingANewBlock_AndAttemptingToVerify_ShouldThrow()
        {
            var rootBlock = Block<MoneyTransferDummyTransaction>.CreateGenesisBlock(new MoneyTransferDummyTransaction("x", "y", 1));
            var newBlock = new Block<MoneyTransferDummyTransaction>(rootBlock, _moneyTransferDummyTransaction, 2);

            Should.Throw<InvalidOperationException>(() => newBlock.VerifyMinedBlock());
        }

        [Fact]
        public void WhenMiningABlockAndSettingValues_AndTheHashDoesntMatchTheExpectedDifficulty_Throws()
        {
            var rootBlock = Block<MoneyTransferDummyTransaction>.CreateGenesisBlock(new MoneyTransferDummyTransaction("x", "y", 1));
            var newBlock = new Block<MoneyTransferDummyTransaction>(rootBlock, _moneyTransferDummyTransaction, 2);

            var miner = new StubMiner<MoneyTransferDummyTransaction>("5b890bf59d1b421aa7e02b6fba2524be", "05EEC80E461F2908F7A6070990699F233383539798E5BAD3F94DBD9096554F5D");
            Should.Throw<InvalidOperationException>(() => miner.MineBlock(newBlock));
        }
    }
}
