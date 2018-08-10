using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public class CpuMiner : IMiner
    {
        private readonly int _numberOfThreads;
        private readonly Block _blockToMine;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private object _lockObject;

        public CpuMiner(int numberOfThreads, Block blockToMine)
        {
            _numberOfThreads = numberOfThreads;
            _blockToMine = blockToMine;
            _cancellationTokenSource = new CancellationTokenSource();
            _lockObject = new object();
        }

        public Block MineBlock()
        {
            var targetHashStart = new string(Block.DefaultBufferCharacter, _blockToMine.Difficulty);

            var token = _cancellationTokenSource.Token;

            var tasks = new List<Task>();

            _blockToMine.SetMiningBeginning();
            for (int i = 0; i < _numberOfThreads; i++)
            {
                tasks.Add(new Task(() => Mine(targetHashStart)));
            }

            foreach (var t in tasks)
            {
                t.Start();
            }

            Task.WaitAll(tasks.ToArray());

            _blockToMine.Verify();

            return _blockToMine;
        }

        private void Mine(string hashStart)
        {
            string GenerateNonce() => Guid.NewGuid().ToString("N");

            var iterations = 0;
            var hash = string.Empty;

            while (!hash.StartsWith(hashStart) && !_cancellationTokenSource.IsCancellationRequested)
            {
                ++iterations;
                var nonce = GenerateNonce();
                hash = Block.CalculateHash(nonce, _blockToMine.Height, _blockToMine.Parent, _blockToMine.RecordedTransaction,
                    iterations, _blockToMine.Difficulty);

                if (hash.StartsWith(hashStart))
                {
                    _cancellationTokenSource.Cancel();
                    lock (_lockObject)
                    {
                        _blockToMine.SetMinedValues(iterations, nonce, hash);
                    }

                    return;
                }
            }
        }

    }
}
