using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public static class CpuMiner
    {
        public static async Task<Block> MineBlock(int numberOfThreads, Block blockToMine)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var targetHashStart = new string(Block.DefaultBufferCharacter, blockToMine.Difficulty);

            var tasks = new ConcurrentBag<Task<(string, string)>> ();

            blockToMine.SetMiningBeginning();
            for (int i = 0; i < numberOfThreads; i++)
            {
                var task = new Task<(string, string)>(() => Mine(targetHashStart, blockToMine, cancellationTokenSource.Token));
                tasks.Add(task);
            }

            foreach (var task in tasks)
            {
                task.Start();
            }
           
            var completedTask = await Task.WhenAny(tasks);
            cancellationTokenSource.Cancel();

            var (nonce, hash) = await completedTask;
            
            blockToMine.SetMinedValues(nonce, hash);
            blockToMine.Verify();

            return blockToMine;
        }

        private static (string nonce, string hash) Mine(string hashStart, Block block, CancellationToken cancellationToken)
        {
            string GenerateNonce() => Guid.NewGuid().ToString("N");
            var hash = string.Empty;
            
            while (!hash.StartsWith(hashStart))
            {

                var nonce = GenerateNonce();
                hash = Block.CalculateHash(nonce, block.Height, block.Parent,
                    block.RecordedTransaction,
                    block.Difficulty);

                cancellationToken.ThrowIfCancellationRequested();
                if (hash.StartsWith(hashStart))
                {
                    return (nonce, hash);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            throw new Exception("Should never reach here");
        }
    }
}
