using System;
using ZChain.Core.Tree;

namespace ZChain
{
    class Program
    {
        static void Main(string[] args)
        {
            var genesisBlock = Block.CreateGenesisBlock(new Transaction("First_Address", "Second_Address", 300), 10);
            genesisBlock.MineBlock();
            Console.WriteLine(genesisBlock);
            Console.WriteLine($"Verified: {Block.VerifyHash(genesisBlock)}");


            var secondBlock = new Block(genesisBlock, new Transaction("Second_Address", "Third_Address", 400), 10);
            secondBlock.MineBlock();

            Console.WriteLine(secondBlock);
            Console.WriteLine($"Verified: {Block.VerifyHash(secondBlock)}");

            var thirdBlock = new Block(secondBlock, new Transaction("ThirdAddress", "FourthAddress", 400), 10);
            thirdBlock.MineBlock();
            Console.WriteLine(thirdBlock);
            Console.WriteLine($"Verified: {Block.VerifyHash(thirdBlock)}");
            Console.ReadLine();
        }
    }
}
