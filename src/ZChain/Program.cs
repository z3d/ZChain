using System;
using System.Diagnostics;
using ZChain.Core.Tree;

namespace ZChain
{
    class Program
    {
        static void Main()
        {
            var threads = 10;
            var difficulty = 6;

            var stopwatch = Stopwatch.StartNew();
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300), difficulty);
            genesisBlock.MineBlock(threads);
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Block hash count: {genesisBlock.Verify()}");
            Console.WriteLine($"Verified: {genesisBlock.Verify()}");


            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), difficulty);
            secondBlock.MineBlock(threads);
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {secondBlock.Verify()}");

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), difficulty);
            thirdBlock.MineBlock(threads);
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {thirdBlock.Verify()}");

            var fourthBlock = new Block(thirdBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            fourthBlock.MineBlock(threads);
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {fourthBlock.Verify()}");

            var fifthBlock = new Block(fourthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            fifthBlock.MineBlock(threads);
            Console.WriteLine(fifthBlock);
            Console.WriteLine($"Verified: {fifthBlock.Verify()}");

            var sixthBlock = new Block(fifthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            sixthBlock.MineBlock(threads);
            Console.WriteLine(sixthBlock);
            Console.WriteLine($"Verified: {sixthBlock.Verify()}");

            var seventhBlock = new Block(sixthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            seventhBlock.MineBlock(threads);
            Console.WriteLine(seventhBlock);
            Console.WriteLine($"Verified: {seventhBlock.Verify()}");

            var éighthBlock = new Block(seventhBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            éighthBlock.MineBlock(threads);
            Console.WriteLine(éighthBlock);
            Console.WriteLine($"Verified: {éighthBlock.Verify()}");

            var ninthBlock = new Block(éighthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            ninthBlock.MineBlock(threads);
            Console.WriteLine(ninthBlock);
            Console.WriteLine($"Verified: {ninthBlock.Verify()}");

            var tenthBlock = new Block(ninthBlock, new Transaction("FourthAddress", "ThirdAddress", 20), difficulty);
            tenthBlock.MineBlock(threads);
            Console.WriteLine(tenthBlock);
            Console.WriteLine($"Verified: {tenthBlock.Verify()}");

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds/1000);

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
