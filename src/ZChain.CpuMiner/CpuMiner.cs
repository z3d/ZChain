﻿using System;
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
        static string GenerateNonce() => Guid.NewGuid().ToString("N");
        var hash = string.Empty;

        while (!hash.StartsWith(hashStart))
        {
            var nonce = GenerateNonce();
            hash = block.CalculateHash(nonce);

            cancellationToken.ThrowIfCancellationRequested(); // This is the standard way to cancel immediately
            if (hash.StartsWith(hashStart))
            {
                return (nonce, hash);
            }
        }

        throw new InvalidOperationException("Unreachable code reached");
    }
}
