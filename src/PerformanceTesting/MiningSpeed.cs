using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZChain.Core.Tree;

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
            var genesisBlock = Block<MoneyTransferDummyTransaction>.CreateGenesisBlock(new MoneyTransferDummyTransaction("First_Address", "Second_Address", 300));

            var secondBlock = new Block<MoneyTransferDummyTransaction>(genesisBlock, new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200), Difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(ThreadCount, secondBlock);
        }
    }
}
