﻿using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core;
using ZChain.CpuMiner;
using ZChain.Tests.Builder;

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
        var genesisBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
            .WithPreviousBlock(null)
            .WithTransaction(new MoneyTransferDummyTransaction("First_Address", "Second_Address", 300))
            .WithDifficulty(difficulty)
            .Build();
        genesisBlock.State.ShouldBe(BlockState.New);
        await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(genesisBlock);
        genesisBlock.VerifyMinedBlock().ShouldBeTrue();
        genesisBlock.Parent.ShouldBeNull();
        genesisBlock.Height.ShouldBe(1);
        genesisBlock.BeginMiningDate.ShouldBeGreaterThan(new DateTimeOffset());
        genesisBlock.State.ShouldBe(BlockState.Mined);

        var secondBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
            .WithPreviousBlock(genesisBlock)
            .WithTransaction(new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200))
            .WithDifficulty(difficulty)
            .Build();
        secondBlock.State.ShouldBe(BlockState.New);
        await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(secondBlock);
        secondBlock.VerifyMinedBlock().ShouldBeTrue();
        secondBlock.Parent.ShouldBe(genesisBlock);
        secondBlock.Height.ShouldBe(2);
        secondBlock.ParentHash.ShouldBe(genesisBlock.Hash);
        secondBlock.State.ShouldBe(BlockState.Mined);

        var thirdBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
            .WithPreviousBlock(secondBlock)
            .WithTransaction(new MoneyTransferDummyTransaction("ThirdAddress", "FourthAddress", 100))
            .WithDifficulty(difficulty)
            .Build();
        thirdBlock.State.ShouldBe(BlockState.New);
        await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(thirdBlock);
        thirdBlock.VerifyMinedBlock().ShouldBeTrue();
        thirdBlock.Parent.ShouldBe(secondBlock);
        thirdBlock.Height.ShouldBe(3);
        thirdBlock.ParentHash.ShouldBe(secondBlock.Hash);
        thirdBlock.State.ShouldBe(BlockState.Mined);
    }
}
