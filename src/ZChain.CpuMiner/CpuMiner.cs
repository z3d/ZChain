using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ZChain.Core;

namespace ZChain.CpuMiner;

public class CpuMiner<T>(int numberOfThreads) : IMiner<T>
{
    private const int NoncesPerBatch = 8;
    private const int NonceDigits = 16;

    private readonly int _numberOfThreads = numberOfThreads;

    public async Task MineBlock(Block<T> blockToMine)
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var tasks = new List<Task<(string, string)>>();

        blockToMine.SetMiningBeginning();
        for (int i = 0; i < _numberOfThreads; i++)
        {
            int firstNonce = i;
            var task = Task.Run(() => Mine(blockToMine, firstNonce, _numberOfThreads, cancellationTokenSource.Token));
            tasks.Add(task);
        }

        var completedTask = await Task.WhenAny(tasks);
#pragma warning disable S6966 // Awaitable method should be used
        cancellationTokenSource.Cancel();
#pragma warning restore S6966 // Awaitable method should be used

        var (nonce, hash) = await completedTask;

        blockToMine.SetMinedValues(nonce, hash);
    }

    private static (string nonce, string hash) Mine(Block<T> block, long firstNonce, int nonceStep, CancellationToken cancellationToken)
    {
        // Each thread walks its own interleaved nonce sequence, so no two threads ever hash the same candidate.
        // Nonces are hashed 8 at a time as fixed-width decimal strings, so the hasher can run them as SIMD lanes.
        // The loop allocates nothing; strings are only built for the winning nonce.
        Span<byte> nonceBytes = stackalloc byte[NonceDigits * NoncesPerBatch];
        for (long nonce = firstNonce; ; nonce += (long)nonceStep * NoncesPerBatch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int i = 0; i < NoncesPerBatch; i++)
            {
                (nonce + i * nonceStep).TryFormat(nonceBytes.Slice(i * NonceDigits, NonceDigits), out _, "D16", CultureInfo.InvariantCulture);
            }

            int found = block.FindNonceSatisfyingDifficulty(nonceBytes, NonceDigits);
            if (found >= 0)
            {
                string nonceString = (nonce + found * nonceStep).ToString("D16", CultureInfo.InvariantCulture);
                return (nonceString, block.CalculateHash(nonceString));
            }
        }
    }
}
