using System;
using System.Collections.Generic;
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
        var targetHashStart = new string(Block<T>.DefaultBufferCharacter, blockToMine.Difficulty);

        var tasks = new List<Task<(string, string)>>();

        blockToMine.SetMiningBeginning();
        for (int i = 0; i < _numberOfThreads; i++)
        {
            var task = Task.Run(() => Mine(targetHashStart, blockToMine, cancellationTokenSource.Token));
            tasks.Add(task);
        }

        var completedTask = await Task.WhenAny(tasks);
#pragma warning disable S6966 // Awaitable method should be used
        cancellationTokenSource.Cancel();
#pragma warning restore S6966 // Awaitable method should be used

        var (nonce, hash) = await completedTask;

        blockToMine.SetMinedValues(nonce, hash);
    }

    private static (string nonce, string hash) Mine(string hashStart, Block<T> block, CancellationToken cancellationToken)
    {
        int iterations = 0;

        while (true)
        {
            string nonce = Guid.NewGuid().ToString("N");
            string hash = block.CalculateHash(nonce);

            if (hash.StartsWith(hashStart, StringComparison.Ordinal))
            {
                return (nonce, hash);
            }

            // Check cancellation every 64 iterations to reduce overhead
            if (++iterations % 64 == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
