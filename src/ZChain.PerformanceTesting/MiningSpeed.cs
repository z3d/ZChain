using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZChain.Core;
using ZChain.CpuMiner;
using ZChain.Core.Builder;

namespace PerformanceTesting
{
    public class MiningSpeed
    {
        [Params(1, 2, 3, 10)]
        public int ThreadCount { get; set; }

        [Params(1, 2, 3)]
        public int Difficulty { get; set; }

        [Benchmark]
        public async Task Mine()
        {
            var genesisTransaction = new TransactionBuilder()
                .WithFromAddress("First_Address")
                .WithToAddress("Second_Address")
                .WithAmount(300m)
                .Build();
            var genesisBlock = new BlockBuilder<MoneyTransferTransaction>()
                .WithPreviousBlock(null)
                .WithTransaction(genesisTransaction)
                .WithDifficulty(Difficulty)
                .Build();

            await new CpuMiner<MoneyTransferTransaction>(ThreadCount).MineBlock(genesisBlock);

            var secondTransaction = new TransactionBuilder()
                .WithFromAddress("Second_Address")
                .WithToAddress("Third_Address")
                .WithAmount(200m)
                .Build();
            var secondBlock = new BlockBuilder<MoneyTransferTransaction>()
                .WithPreviousBlock(genesisBlock)
                .WithTransaction(secondTransaction)
                .WithDifficulty(Difficulty)
                .Build();

            await new CpuMiner<MoneyTransferTransaction>(ThreadCount).MineBlock(secondBlock);
        }
    }
}
