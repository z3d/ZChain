using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public static class CpuMiner<T>
    {
        public static async Task MineBlock(int numberOfThreads, Block<T> blockToMine)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var targetHashStart = new string(Block<T>.DefaultBufferCharacter, blockToMine.Difficulty);

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
        }

        private static (string nonce, string hash) Mine(string hashStart, Block<T> block, CancellationToken cancellationToken)
        {
            string GenerateNonce() => Guid.NewGuid().ToString("N");
            var hash = string.Empty;
            
            while (!hash.StartsWith(hashStart))
            {
                var nonce = GenerateNonce();
                hash = block.CalculateHash(nonce);

                cancellationToken.ThrowIfCancellationRequested();
                if (hash.StartsWith(hashStart))
                {
                    return (nonce, hash);
                }
            }

            cancellationToken.ThrowIfCancellationRequested(); // This is the standard way to cancel immediately
            throw new InvalidOperationException("Unreachable code reached");
        }
    }
}
