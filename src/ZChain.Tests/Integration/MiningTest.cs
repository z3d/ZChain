using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core;
using ZChain.CpuMiner;
using ZChain.Core.Builder;

namespace ZChain.Tests.Integration;
public class MiningTest
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 3)]
    [InlineData(2, 1)]
    [InlineData(2, 3)]
    async Task GivenThreeBlocks_WhenMining_TheyAreMinedCorrectly(int threads, int difficulty)
    {
        var genesisTransaction = new TransactionBuilder()
            .WithFromAddress("First_Address")
            .WithToAddress("Second_Address")
            .WithAmount(300m)
            .Build();
        var genesisBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(null)
            .WithTransaction(genesisTransaction)
            .WithDifficulty(difficulty)
            .Build();
        genesisBlock.State.ShouldBe(BlockState.New);

        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(genesisBlock);
        genesisBlock.VerifyMinedBlock().ShouldBeTrue();
        genesisBlock.Parent.ShouldBeNull();
        genesisBlock.Height.ShouldBe(1);
        genesisBlock.BeginMiningDate.ShouldBeGreaterThan(new DateTimeOffset());
        genesisBlock.State.ShouldBe(BlockState.Mined);

        var secondTransaction = new TransactionBuilder()
            .WithFromAddress("Second_Address")
            .WithToAddress("Third_Address")
            .WithAmount(200m)
            .Build();
        var secondBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(genesisBlock)
            .WithTransaction(secondTransaction)
            .WithDifficulty(difficulty)
            .Build();
        secondBlock.State.ShouldBe(BlockState.New);

        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(secondBlock);
        secondBlock.VerifyMinedBlock().ShouldBeTrue();
        secondBlock.Parent.ShouldBe(genesisBlock);
        secondBlock.Height.ShouldBe(2);
        secondBlock.ParentHash.ShouldBe(genesisBlock.Hash);
        secondBlock.State.ShouldBe(BlockState.Mined);

        var thirdTransaction = new TransactionBuilder()
            .WithFromAddress("ThirdAddress")
            .WithToAddress("FourthAddress")
            .WithAmount(100m)
            .Build();
        var thirdBlock = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(secondBlock)
            .WithTransaction(thirdTransaction)
            .WithDifficulty(difficulty)
            .Build();
        thirdBlock.State.ShouldBe(BlockState.New);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(thirdBlock);
        thirdBlock.VerifyMinedBlock().ShouldBeTrue();
        thirdBlock.Parent.ShouldBe(secondBlock);
        thirdBlock.Height.ShouldBe(3);
        thirdBlock.ParentHash.ShouldBe(secondBlock.Hash);
        thirdBlock.State.ShouldBe(BlockState.Mined);
    }
}
