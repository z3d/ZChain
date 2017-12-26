using System;
using ZChain.Core.Tree;

namespace ZChain
{
    class Program
    {
        static void Main(string[] args)
        {
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300), 5);
            genesisBlock.MineBlock();
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {Block.Verify(genesisBlock)}");


            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 200), 5);
            secondBlock.MineBlock();
            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {Block.Verify(secondBlock)}");

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 100), 5);
            thirdBlock.MineBlock();
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {Block.Verify(thirdBlock)}");

            var fourthBlock = new Block(thirdBlock, new Transaction("FourthAddress", "ThirdAddress", 20), 5);
            fourthBlock.MineBlock();
            Console.WriteLine(fourthBlock);
            Console.WriteLine($"Verified: {Block.Verify(fourthBlock)}");

            Console.WriteLine("Done, press any key to continue");
            Console.ReadLine();
        }
    }
}
