using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ZChain.Core;

namespace ZChain.CpuMiner;

public class CpuMiner<T>(int numberOfThreads) : IMiner<T>
{
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
        // The loop allocates nothing; strings are only built for the winning nonce.
        Span<byte> nonceBytes = stackalloc byte[20];
        for (long nonce = firstNonce; ; nonce += nonceStep)
        {
            cancellationToken.ThrowIfCancellationRequested();

            nonce.TryFormat(nonceBytes, out int nonceLength, provider: CultureInfo.InvariantCulture);
            if (block.SatisfiesDifficulty(nonceBytes[..nonceLength]))
            {
                string nonceString = nonce.ToString(CultureInfo.InvariantCulture);
                return (nonceString, block.CalculateHash(nonceString));
            }
        }
    }
}
