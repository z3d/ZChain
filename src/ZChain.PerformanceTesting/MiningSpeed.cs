using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZChain.Core;
using ZChain.CpuMiner;
using ZChain.Tests.Builder;

namespace PerformanceTesting
{
    public class MiningSpeed
    {
        [Params(1,2,3,10)]
        public int ThreadCount { get; set; }

        [Params(1, 2, 3)]
        public int Difficulty { get; set; }

        [Benchmark]
        public async Task Mine()
        {
            var genesisBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(null)
                .WithTransaction(new MoneyTransferDummyTransaction("First_Address", "Second_Address", 300))
                .WithDifficulty(Difficulty)
                .Build();

            await new CpuMiner<MoneyTransferDummyTransaction>(ThreadCount).MineBlock(genesisBlock);

            var secondBlock = new BlockBuilder<MoneyTransferDummyTransaction>()
                .WithPreviousBlock(genesisBlock)
                .WithTransaction(new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200))
                .WithDifficulty(Difficulty)
                .Build();

            await new CpuMiner<MoneyTransferDummyTransaction>(ThreadCount).MineBlock(secondBlock);
        }
    }
}
