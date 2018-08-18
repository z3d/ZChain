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

        class StubMiner<T> : IMiner<T>
        {
            public Task MineBlock(Block<T> blockToMine)
            {
                blockToMine.SetMiningBeginning();
                blockToMine.SetMinedValues("3ae359e12e9a4ad19559fbffe8f3c15a", "0041AD4CC3B9C508336BA949D302076E0A47DAB769B7D2A6F216791A913A4036");
                return Task.CompletedTask;
            }
        }

        public BlockTests()
        {
            var rootBlock = Block<MoneyTransferDummyTransaction>.CreateGenesisBlock(new MoneyTransferDummyTransaction("x", "y", 1));
            _firstChild = new Block<MoneyTransferDummyTransaction>(rootBlock, new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200), 2);
        }

        [Fact]
        public void WhenConstructingABlock_AndTheParentBlockIsNull_ShouldThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Should.Throw<ArgumentNullException>(() => new Block<MoneyTransferDummyTransaction>(null, new MoneyTransferDummyTransaction("x","y",2), 1));
        }

        [Fact]
        public void WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow()
        {
            var miner = new StubMiner<MoneyTransferDummyTransaction>();
            miner.MineBlock(_firstChild);

            Should.Throw<InvalidOperationException>(() => miner.MineBlock(_firstChild));
        }
    }
}
