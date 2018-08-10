﻿using System;
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
        async void GivenThreeBlocks_WhenMining_TheyAreMinedCorrectly(int threads, int difficulty)
        {
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300));
            genesisBlock.State.ShouldBe(BlockState.Mined);
            genesisBlock.Verify().ShouldBeTrue();
            genesisBlock.Parent.ShouldBeNull();
            genesisBlock.Height.ShouldBe(0);
            genesisBlock.BeginMiningDate.ShouldBeGreaterThan(new DateTimeOffset());
            genesisBlock.State.ShouldBe(BlockState.Mined);

            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), difficulty);
            secondBlock.State.ShouldBe(BlockState.New);
            IMiner secondBlockMiner = new CpuMiner(threads, secondBlock);
            await secondBlockMiner.MineBlock();
            secondBlock.Verify().ShouldBeTrue();
            secondBlock.Parent.ShouldBe(genesisBlock);
            secondBlock.Height.ShouldBe(1);
            secondBlock.ParentHash.ShouldBe(genesisBlock.Hash);
            secondBlock.State.ShouldBe(BlockState.Mined);

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), difficulty);
            thirdBlock.State.ShouldBe(BlockState.New);
            IMiner thirdBlockMiner = new CpuMiner(threads, thirdBlock);
            await thirdBlockMiner.MineBlock();
            thirdBlock.Verify().ShouldBeTrue();
            thirdBlock.Parent.ShouldBe(secondBlock);
            thirdBlock.Height.ShouldBe(2);
            thirdBlock.ParentHash.ShouldBe(secondBlock.Hash);
            thirdBlock.State.ShouldBe(BlockState.Mined);
        }
    }
}
