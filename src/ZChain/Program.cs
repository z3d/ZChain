using System;
using System.Diagnostics;
using ZChain.Core.Tree;

namespace ZChain
{
    class Program
    {
        static void Main()
        {
            var threads = 11;
            var difficulty = 8;

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

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds/1000);

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
