using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public class CpuMiner : IMiner
    {
        private readonly int _numberOfThreads;
        private readonly Block _blockToMine;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly object _lockObject;

        public CpuMiner(int numberOfThreads, Block blockToMine)
        {
            _numberOfThreads = numberOfThreads;
            _blockToMine = blockToMine;
            _cancellationTokenSource = new CancellationTokenSource();
            _lockObject = new object();
        }

        public async Task<Block> MineBlock()
        {
            var targetHashStart = new string(Block.DefaultBufferCharacter, _blockToMine.Difficulty);

            var tasks = new List<Task<(string, string)>>();

            _blockToMine.SetMiningBeginning();
            for (int i = 0; i < _numberOfThreads; i++)
            {
                tasks.Add(new Task<(string,string)>(() => Mine(targetHashStart, _blockToMine, _cancellationTokenSource.Token)));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }
           
            var completedTask = await Task.WhenAny(tasks);
            var (nonce, hash) = await completedTask;
            if (hash == null || nonce == null)
            {
                throw new Exception($"{nameof(hash)} is {hash} ");
            }
            _blockToMine.SetMinedValues(nonce, hash);
            _blockToMine.Verify();

            return _blockToMine;
        }

        private (string nonce, string hash) Mine(string hashStart, Block block, CancellationToken token)
        {
            string GenerateNonce() => Guid.NewGuid().ToString("N");
            var hash = string.Empty;

            while (!hash.StartsWith(hashStart) && !_cancellationTokenSource.IsCancellationRequested)
            {
                var nonce = GenerateNonce();
                hash = Block.CalculateHash(nonce, block.Height, block.Parent,
                    block.RecordedTransaction,
                    block.Difficulty);

                if (hash.StartsWith(hashStart))
                {
                    return (nonce, hash);
                }
            }

            token.ThrowIfCancellationRequested();
            return (null, null);
        }
    }
}
