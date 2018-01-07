using System;
using ZChain.Core.Tree;

namespace ZChain
{
    class Program
    {
        static void Main()
        {
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300), 5);
            genesisBlock.MineBlock();
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {genesisBlock.Verify()}");


            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), 5);
            secondBlock.MineBlock();
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {secondBlock.Verify()}");

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), 5);
            thirdBlock.MineBlock();
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {thirdBlock.Verify()}");

            var fourthBlock = new Block(thirdBlock, new Transaction("FourthAddress", "ThirdAddress", 20), 5);
            fourthBlock.MineBlock();
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {fourthBlock.Verify()}");

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
