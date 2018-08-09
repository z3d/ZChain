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

        public CpuMiner(int numberOfThreads)
        {
            _numberOfThreads = numberOfThreads;
        }

        public Block MineBlock(Block blockToMine)
        {
            var cts = new CancellationTokenSource();

            void Mine(string hashStart)
            {
                string GenerateNonce()
                {
                    return Guid.NewGuid().ToString("N");
                }

                var iterations = 0;
                var nonce = string.Empty;
                var minedDate = DateTimeOffset.Now;
                var hash = string.Empty;

                while (!hash.StartsWith(hashStart) && !cts.IsCancellationRequested)
                {
                    ++iterations;
                    nonce = GenerateNonce();
                    minedDate = DateTimeOffset.Now;
                    hash = BlockExtensions.CalculateHash(nonce, blockToMine.Height,blockToMine.Parent,blockToMine.RecordedTransaction,
                        minedDate, iterations, blockToMine.Difficulty);
                }

                if (cts.IsCancellationRequested)
                {
                    return;
                }

                cts.Cancel();

                blockToMine.SetMinedValues(iterations, nonce, minedDate, hash);
            }

            var targetHashStart = new string(Block.DefaultBufferCharacter, blockToMine.Difficulty);

            var token = cts.Token;

            var tasks = new List<Task>();

            blockToMine.SetMiningBeginning();
            for (int i = 0; i < _numberOfThreads; i++)
            {
                tasks.Add(new Task(() => Mine(targetHashStart), token));
            }

            foreach (var t in tasks)
            {
                t.Start();
            }

            Task.WaitAny(tasks.ToArray());
            blockToMine.Verify();

            return blockToMine;
        }
    }
}
