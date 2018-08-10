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
            var threads = 10;
            var difficulty = 6;

            var stopwatch = Stopwatch.StartNew();
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300));
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {genesisBlock.Verify()}");


            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), difficulty);
            await CpuMiner.MineBlock(threads,secondBlock);
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {secondBlock.Verify()}");

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), difficulty);
            await CpuMiner.MineBlock(threads, thirdBlock);
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {thirdBlock.Verify()}");

            var fourthBlock = new Block(thirdBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads, fourthBlock);
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {fourthBlock.Verify()}");

            var fifthBlock = new Block(fourthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads, fifthBlock);
            Console.WriteLine(fifthBlock);
            Console.WriteLine($"Verified: {fifthBlock.Verify()}");

            var sixthBlock = new Block(fifthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads, sixthBlock);
            Console.WriteLine(sixthBlock);
            Console.WriteLine($"Verified: {sixthBlock.Verify()}");

            var seventhBlock = new Block(sixthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads, seventhBlock);
            Console.WriteLine(seventhBlock);
            Console.WriteLine($"Verified: {seventhBlock.Verify()}");

            var eighthBlock = new Block(seventhBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads,eighthBlock);
            Console.WriteLine($"Verified: {eighthBlock.Verify()}");

            var ninthBlock = new Block(eighthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads, ninthBlock);
            Console.WriteLine(ninthBlock);
            Console.WriteLine($"Verified: {ninthBlock.Verify()}");

            var tenthBlock = new Block(ninthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            await CpuMiner.MineBlock(threads, tenthBlock);
            Console.WriteLine(tenthBlock);
            Console.WriteLine($"Verified: {tenthBlock.Verify()}");

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds/1000);

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
