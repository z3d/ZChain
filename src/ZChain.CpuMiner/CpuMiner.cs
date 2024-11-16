using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZChain.Core;

namespace ZChain.CpuMiner;

public class CpuMiner<T>
{
    private readonly int _numberOfThreads;
    private readonly int _batchSize;

    public CpuMiner(int numberOfThreads, int batchSize = 1000)
    {
        _numberOfThreads = numberOfThreads;
        _batchSize = batchSize;
    }

    public async Task MineBlock(Block<T> blockToMine)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var targetHashStart = new string(Block<T>.DefaultBufferCharacter, blockToMine.Difficulty);

        blockToMine.SetMiningBeginning();
        
        var tasks = new List<Task<(string, string)>>(_numberOfThreads);
        var nonceRangePerThread = uint.MaxValue / (ulong)_numberOfThreads;

        for (int i = 0; i < _numberOfThreads; i++)
        {
            var startNonce = (ulong)i * nonceRangePerThread;
            var endNonce = ((ulong)(i + 1)) * nonceRangePerThread - 1;
            
            var task = Task.Run(() => MineBatch(
                targetHashStart, 
                blockToMine, 
                startNonce, 
                endNonce,
                _batchSize, 
                cancellationTokenSource.Token));
                
            tasks.Add(task);
        }

        var completedTask = await Task.WhenAny(tasks);
        cancellationTokenSource.Cancel();

        var (nonce, hash) = await completedTask;
        blockToMine.SetMinedValues(nonce, hash);
    }

    private static (string, string) MineBatch(
        string targetHashStart,
        Block<T> block,
        ulong startNonce,
        ulong endNonce, 
        int batchSize,
        CancellationToken token)
    {
        var sb = new StringBuilder();
        var currentNonce = startNonce;

        while (currentNonce < endNonce && !token.IsCancellationRequested)
        {
            for (int i = 0; i < batchSize && currentNonce < endNonce; i++, currentNonce++)
            {
                var nonceStr = currentNonce.ToString();
                var hash = block.CalculateHash(nonceStr);
                
                if (hash.StartsWith(targetHashStart))
                {
                    return (nonceStr, hash);
                }
            }
        }

        return (string.Empty, string.Empty);
    }
}
