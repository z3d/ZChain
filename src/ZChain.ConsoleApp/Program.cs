using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZChain.Core;
using ZChain.Core.Builder;
using ZChain.CpuMiner;
using ZChain.Hashers;

namespace ZChain.ConsoleApp;

static class Program
{
    static async Task Main()
    {
        var threads = 11;
        var difficulty = 4;
        var hasher = new Sha256Hasher();

        var stopwatch = Stopwatch.StartNew();

        var genesisTransaction = new TransactionBuilder()
            .WithFromAddress("First_Address")
            .WithToAddress("Second_Address")
            .WithAmount(300)
            .Build();
        var genesisBlock = new Block<MoneyTransferTransaction>(null, genesisTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(genesisBlock);
        Console.WriteLine(genesisBlock);
        Console.WriteLine($"Verified: {genesisBlock.VerifyMinedBlock()}");

        var secondTransaction = new TransactionBuilder()
            .WithFromAddress("Second_Address")
            .WithToAddress("Third_Address")
            .WithAmount(200)
            .Build();
        var secondBlock = new Block<MoneyTransferTransaction>(genesisBlock, secondTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(secondBlock);
        Console.WriteLine(secondBlock);
        Console.WriteLine($"Verified: {secondBlock.VerifyMinedBlock()}");

        var thirdTransaction = new TransactionBuilder()
            .WithFromAddress("ThirdAddress")
            .WithToAddress("FourthAddress")
            .WithAmount(100)
            .Build();
        var thirdBlock = new Block<MoneyTransferTransaction>(secondBlock, thirdTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(thirdBlock);
        Console.WriteLine(thirdBlock);
        Console.WriteLine($"Verified: {thirdBlock.VerifyMinedBlock()}");

        var fourthTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var fourthBlock = new Block<MoneyTransferTransaction>(thirdBlock, fourthTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(fourthBlock);
        Console.WriteLine(fourthBlock);
        Console.WriteLine($"Verified: {fourthBlock.VerifyMinedBlock()}");

        var fifthTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var fifthBlock = new Block<MoneyTransferTransaction>(fourthBlock, fifthTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(fifthBlock);
        Console.WriteLine(fifthBlock);
        Console.WriteLine($"Verified: {fifthBlock.VerifyMinedBlock()}");

        var sixthTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var sixthBlock = new Block<MoneyTransferTransaction>(fifthBlock, sixthTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(sixthBlock);
        Console.WriteLine(sixthBlock);
        Console.WriteLine($"Verified: {sixthBlock.VerifyMinedBlock()}");

        var seventhTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var seventhBlock = new Block<MoneyTransferTransaction>(sixthBlock, seventhTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(seventhBlock);
        Console.WriteLine(seventhBlock);
        Console.WriteLine($"Verified: {seventhBlock.VerifyMinedBlock()}");

        var eighthTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var eighthBlock = new Block<MoneyTransferTransaction>(seventhBlock, eighthTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(eighthBlock);
        Console.WriteLine(eighthBlock);
        Console.WriteLine($"Verified: {eighthBlock.VerifyMinedBlock()}");

        var ninthTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var ninthBlock = new Block<MoneyTransferTransaction>(eighthBlock, ninthTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(ninthBlock);
        Console.WriteLine(ninthBlock);
        Console.WriteLine($"Verified: {ninthBlock.VerifyMinedBlock()}");

        var tenthTransaction = new TransactionBuilder()
            .WithFromAddress("FourthAddress")
            .WithToAddress("ThirdAddress")
            .WithAmount(20)
            .Build();
        var tenthBlock = new Block<MoneyTransferTransaction>(ninthBlock, tenthTransaction, difficulty, hasher);
        await new CpuMiner<MoneyTransferTransaction>(threads).MineBlock(tenthBlock);
        Console.WriteLine(tenthBlock);
        Console.WriteLine($"Verified: {tenthBlock.VerifyMinedBlock()}");

        stopwatch.Stop();
        Console.WriteLine($"Time taken for execution: {stopwatch.Elapsed.TotalSeconds} seconds");

        Console.WriteLine("Done, press any key to continue");
        Console.ReadLine();
    }
}
