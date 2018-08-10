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
            var difficulty = 8;

            var stopwatch = Stopwatch.StartNew();
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300));
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {genesisBlock.Verify()}");


            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), difficulty);
            IMiner secondBlockMiner = new CpuMiner(threads, secondBlock);
            await secondBlockMiner.MineBlock();
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {secondBlock.Verify()}");

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), difficulty);
            IMiner thirdBlockMiner = new CpuMiner(threads, thirdBlock);
            await thirdBlockMiner.MineBlock();
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {thirdBlock.Verify()}");

            var fourthBlock = new Block(thirdBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner fourthBlockMiner = new CpuMiner(threads, fourthBlock);
            await fourthBlockMiner.MineBlock();
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {fourthBlock.Verify()}");

            var fifthBlock = new Block(fourthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner fifthBlockMiner = new CpuMiner(threads, fifthBlock);
            await fifthBlockMiner.MineBlock();
            Console.WriteLine(fifthBlock);
            Console.WriteLine($"Verified: {fifthBlock.Verify()}");

            var sixthBlock = new Block(fifthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner sixthBlockMiner = new CpuMiner(threads, sixthBlock);
            await sixthBlockMiner.MineBlock();
            Console.WriteLine(sixthBlock);
            Console.WriteLine($"Verified: {sixthBlock.Verify()}");

            var seventhBlock = new Block(sixthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner seventhBlockMiner = new CpuMiner(threads, seventhBlock);
            await seventhBlockMiner.MineBlock();
            Console.WriteLine(seventhBlock);
            Console.WriteLine($"Verified: {seventhBlock.Verify()}");

            var éighthBlock = new Block(seventhBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner eigthBlockMiner = new CpuMiner(threads, éighthBlock);
            await eigthBlockMiner.MineBlock();
            Console.WriteLine(éighthBlock);
            Console.WriteLine($"Verified: {éighthBlock.Verify()}");

            var ninthBlock = new Block(éighthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner ninthBlockMiner = new CpuMiner(threads, ninthBlock);
            await ninthBlockMiner.MineBlock();
            Console.WriteLine(ninthBlock);
            Console.WriteLine($"Verified: {ninthBlock.Verify()}");

            var tenthBlock = new Block(ninthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            IMiner tenthBlockMiner = new CpuMiner(threads, tenthBlock);
            await tenthBlockMiner.MineBlock();
            Console.WriteLine(tenthBlock);
            Console.WriteLine($"Verified: {tenthBlock.Verify()}");

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds/1000);

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
