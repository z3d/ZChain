using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZChain.Core;

namespace ZChain
{
    class Program
    {
        static async Task Main()
        {
            var threads = 11;
            var difficulty = 4;

            var stopwatch = Stopwatch.StartNew();
            var genesisBlock = new Block<MoneyTransferDummyTransaction>(null, new MoneyTransferDummyTransaction("First_Address", "Second_Address", 300), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(genesisBlock);
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {genesisBlock.VerifyMinedBlock()}");

            var secondBlock = new Block<MoneyTransferDummyTransaction>(genesisBlock, new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(secondBlock);
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {secondBlock.VerifyMinedBlock()}");

            var thirdBlock = new Block<MoneyTransferDummyTransaction>(secondBlock, new MoneyTransferDummyTransaction("ThirdAddress", "FourthAddress", 100), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock( thirdBlock);
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {thirdBlock.VerifyMinedBlock()}");

            var fourthBlock = new Block<MoneyTransferDummyTransaction>(thirdBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(fourthBlock);
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {fourthBlock.VerifyMinedBlock()}");

            var fifthBlock = new Block<MoneyTransferDummyTransaction>(fourthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(fifthBlock);
            Console.WriteLine(fifthBlock);
            Console.WriteLine($"Verified: {fifthBlock.VerifyMinedBlock()}");

            var sixthBlock = new Block<MoneyTransferDummyTransaction>(fifthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(sixthBlock);
            Console.WriteLine(sixthBlock);
            Console.WriteLine($"Verified: {sixthBlock.VerifyMinedBlock()}");

            var seventhBlock = new Block<MoneyTransferDummyTransaction>(sixthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(seventhBlock);
            Console.WriteLine(seventhBlock);
            Console.WriteLine($"Verified: {seventhBlock.VerifyMinedBlock()}");

            var eighthBlock = new Block<MoneyTransferDummyTransaction>(seventhBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(eighthBlock);
            Console.WriteLine(eighthBlock);
            Console.WriteLine($"Verified: {eighthBlock.VerifyMinedBlock()}");

            var ninthBlock = new Block<MoneyTransferDummyTransaction>(eighthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(ninthBlock);
            Console.WriteLine(ninthBlock);
            Console.WriteLine($"Verified: {ninthBlock.VerifyMinedBlock()}");

            var tenthBlock = new Block<MoneyTransferDummyTransaction>(ninthBlock, new MoneyTransferDummyTransaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await new CpuMiner<MoneyTransferDummyTransaction>(threads).MineBlock(tenthBlock);
            Console.WriteLine(tenthBlock);
            Console.WriteLine($"Verified: {tenthBlock.VerifyMinedBlock()}");

            stopwatch.Stop();
            Console.WriteLine($"Time taken for execution: {stopwatch.Elapsed.TotalSeconds} seconds");

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
