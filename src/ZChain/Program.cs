using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZChain.Core.Tree;

namespace ZChain
{
    class Program
    {
        static async Task Main()
        {
            var threads = 9;
            var difficulty = 6;

            var stopwatch = Stopwatch.StartNew();
            var genesisBlock = Block<MoneyTransferDummyTransaction>.CreateGenesisBlock(new MoneyTransferDummyTransaction("First_Address", "Second_Address", 300));
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {genesisBlock.Verify()}");

            var secondBlock = new Block<MoneyTransferDummyTransaction>(genesisBlock, new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads,secondBlock);
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {secondBlock.Verify()}");

            var thirdBlock = new Block<MoneyTransferDummyTransaction>(secondBlock, new MoneyTransferDummyTransaction("ThirdAddress", "FourthAddress", 100), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, thirdBlock);
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {thirdBlock.Verify()}");

            var fourthBlock = new Block<MoneyTransferDummyTransaction>(thirdBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, fourthBlock);
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {fourthBlock.Verify()}");

            var fifthBlock = new Block<MoneyTransferDummyTransaction>(fourthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, fifthBlock);
            Console.WriteLine(fifthBlock);
            Console.WriteLine($"Verified: {fifthBlock.Verify()}");

            var sixthBlock = new Block<MoneyTransferDummyTransaction>(fifthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, sixthBlock);
            Console.WriteLine(sixthBlock);
            Console.WriteLine($"Verified: {sixthBlock.Verify()}");

            var seventhBlock = new Block<MoneyTransferDummyTransaction>(sixthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, seventhBlock);
            Console.WriteLine(seventhBlock);
            Console.WriteLine($"Verified: {seventhBlock.Verify()}");

            var eighthBlock = new Block<MoneyTransferDummyTransaction>(seventhBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads,eighthBlock);
            Console.WriteLine(eighthBlock);
            Console.WriteLine($"Verified: {eighthBlock.Verify()}");

            var ninthBlock = new Block<MoneyTransferDummyTransaction>(eighthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, ninthBlock);
            Console.WriteLine(ninthBlock);
            Console.WriteLine($"Verified: {ninthBlock.Verify()}");

            var tenthBlock = new Block<MoneyTransferDummyTransaction>(ninthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner<MoneyTransferDummyTransaction>.MineBlock(threads, tenthBlock);
            Console.WriteLine(tenthBlock);
            Console.WriteLine($"Verified: {tenthBlock.Verify()}");

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds/1000);

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
