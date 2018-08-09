using Shouldly;
using Xunit;
using ZChain.Core.Tree;

namespace ZChain.Tests.Integration
{
    public class MiningTest
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 3)]
        [InlineData(2,1)]
        [InlineData(2, 3)]
        void GivenThreeBlocks_WhenMining_TheyAreMinedCorrectly(int threads, int difficulty)
        {
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300), difficulty);
            genesisBlock.State.ShouldBe(BlockState.New);
            genesisBlock.MineBlock(threads);
            genesisBlock.Verify().ShouldBeTrue();
            genesisBlock.Parent.ShouldBeNull();
            genesisBlock.Height.ShouldBe(0);
            genesisBlock.IterationsToMinedResult.ShouldBe(0);
            genesisBlock.MinedDate.ShouldBeGreaterThan(genesisBlock.ReceivedDate);
            genesisBlock.State.ShouldBe(BlockState.Mined);

            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), difficulty);
            secondBlock.State.ShouldBe(BlockState.New);
            secondBlock.MineBlock(threads);
            secondBlock.Verify().ShouldBeTrue();
            secondBlock.Parent.ShouldBe(genesisBlock);
            secondBlock.Height.ShouldBe(1);
            secondBlock.MinedDate.ShouldBeGreaterThan(secondBlock.ReceivedDate);
            secondBlock.ParentHash.ShouldBe(genesisBlock.Hash);
            secondBlock.State.ShouldBe(BlockState.Mined);

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), difficulty);
            thirdBlock.State.ShouldBe(BlockState.New);
            thirdBlock.MineBlock(threads);
            thirdBlock.Verify().ShouldBeTrue();
            thirdBlock.Parent.ShouldBe(secondBlock);
            thirdBlock.Height.ShouldBe(2);
            thirdBlock.MinedDate.ShouldBeGreaterThan(thirdBlock.ReceivedDate);
            thirdBlock.ParentHash.ShouldBe(secondBlock.Hash);
            thirdBlock.State.ShouldBe(BlockState.Mined);
        }

    }
}
