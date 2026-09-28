using System;
using System.Globalization;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZChain.Core;
using ZChain.Core.Builder;
using ZChain.Hashers;

namespace ZChain.PerformanceTesting;

// Mining is a lottery, so time-to-mine-a-block measures the draw, not the miner.
// This hashes a fixed number of nonces through the same path CpuMiner uses and reports nanoseconds per hash.
public class HashRate
{
    private const int HashesPerInvoke = 61_440; // divisible by 8 and by every ThreadCount below
    private const int NoncesPerBatch = 8;
    private const int NonceDigits = 16;
    private const int UnreachableDifficulty = 64; // 64 zero nibbles: no 32-byte hash satisfies it, so every nonce is hashed

    [Params(1, 2, 3, 10)]
    public int ThreadCount { get; set; }

    private Block<MoneyTransferTransaction> _block = null!;

    [GlobalSetup]
    public void Setup()
    {
        var transaction = new TransactionBuilder()
            .WithFromAddress("First_Address")
            .WithToAddress("Second_Address")
            .WithAmount(300m)
            .Build();
        _block = new BlockBuilder<MoneyTransferTransaction>()
            .WithPreviousBlock(null)
            .WithTransaction(transaction)
            .WithDifficulty(UnreachableDifficulty)
            .WithHasher(new Sha256Hasher())
            .Build();
    }

    [Benchmark(OperationsPerInvoke = HashesPerInvoke)]
    public void Hash()
    {
        int noncesPerThread = HashesPerInvoke / ThreadCount;
        Parallel.For(0, ThreadCount, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, thread => HashRange(thread * noncesPerThread, noncesPerThread));
    }

    private void HashRange(long firstNonce, int count)
    {
        Span<byte> nonces = stackalloc byte[NonceDigits * NoncesPerBatch];
        for (long nonce = firstNonce; nonce < firstNonce + count; nonce += NoncesPerBatch)
        {
            for (int i = 0; i < NoncesPerBatch; i++)
            {
                (nonce + i).TryFormat(nonces.Slice(i * NonceDigits, NonceDigits), out _, "D16", CultureInfo.InvariantCulture);
            }

            if (_block.FindNonceSatisfyingDifficulty(nonces, NonceDigits) >= 0)
            {
                throw new InvalidOperationException("A nonce satisfied an unreachable difficulty");
            }
        }
    }
}
