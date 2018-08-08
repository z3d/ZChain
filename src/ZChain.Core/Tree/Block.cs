using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public class Block
    {
        private static readonly char BufferCharacter = '0';
        CancellationTokenSource _cts;

        public static Block CreateGenesisBlock(ITransaction recordedTransaction, int difficulty)
        {
            return new Block(recordedTransaction, difficulty)
            {
                Hash = new string(BufferCharacter, 32),
                Parent = null,
                Height = 0
            };
        }

        public Block(Block parent, ITransaction recordedTransaction, int difficulty): this(recordedTransaction, difficulty)
        {
            _cts = new CancellationTokenSource();

            Parent = parent ?? throw new ArgumentNullException(
                         $"Parent of block cannot be null. Create genesis block using factory method and use as the root.");
            ParentHash = parent.Hash;
            Height = parent.Height + 1;
        }

        private Block(ITransaction recordedTransaction, int difficulty)
        {
            if(difficulty <= 0)
            {
                throw new Exception("Difficulty must exceed 0");
            }

            RecordedTransaction = recordedTransaction;
            Difficulty = difficulty;
            State = BlockState.New;
            Hash = "NEW_BLOCK";
            IterationsToMinedResult = 0;
        }

        public Block Parent { get; private set; }
        public ITransaction RecordedTransaction { get; private set; }
        public int Difficulty { get; private set; }
        public BlockState State { get; private set; }
        public int IterationsToMinedResult { get; private set; }

        public string Hash { get; private set; }
        public string ParentHash { get; private set; }

        public string Nonce { get; private set; }

        public DateTimeOffset ReceivedDate { get; private set; }

        public DateTimeOffset MinedDate { get; private set; }

        public long Height { get; private set; }

        public void MineBlock(int numberOfThreads = 1)
        {
            void Mine(string hashStart)
            {
                var iterations = 0;
                var nonce = string.Empty;
                var minedDate = DateTimeOffset.Now;
                var hash = string.Empty;

                while (!hash.StartsWith(hashStart) && !_cts.IsCancellationRequested)
                {
                    ++iterations;
                    nonce = GenerateNonce();
                    minedDate = DateTimeOffset.Now;
                    hash = CalculateHash(nonce, Height, Parent, RecordedTransaction, minedDate, iterations, Difficulty);
                }

                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                _cts?.Cancel();

                IterationsToMinedResult = iterations;
                Nonce = nonce;
                MinedDate = minedDate;
                Hash = hash;
            }

            if (State != BlockState.New)
            {
                throw new Exception("Cannot remine a block");
            }

            State = BlockState.Mining;
            var targetHashStart = new string(BufferCharacter, Difficulty);
            ReceivedDate = DateTimeOffset.Now;

            if (Height != 0)
            {
                var token = _cts.Token;

                var tasks = new List<Task>();


                for (int i = 0; i < numberOfThreads; i++)
                {
                    tasks.Add(new Task(() => Mine(targetHashStart), token));
                }

                foreach (var t in tasks)
                {
                   t.Start();
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                MinedDate = DateTimeOffset.Now;
            }

            State = BlockState.Mined;
            Verify(this);
        }

        public override string ToString()
        {
            return
                $"Hash: {Hash} Parent Hash: {Parent?.Hash} Height: {Height} Transaction: {RecordedTransaction} " +
                $"Nonce: {Nonce} Difficulty: {Difficulty} Received Date: {ReceivedDate} Mined Date: {MinedDate} Iterations to mine Result: {IterationsToMinedResult}, " +
                $" Seconds to hash result: {(MinedDate - ReceivedDate).TotalSeconds}";
        }

        private string GenerateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        public bool Verify()
        {
            return Verify(this);
        }

        private static bool Verify(Block blockToVerify)
        {
            if (blockToVerify.Height != 0)
            {
                Verify(blockToVerify.Parent);
            }

            else if (blockToVerify.Hash != new string(BufferCharacter, 32))
            {
                throw new Exception($"Genesis block hash incorrect");
            }
            
            Debug.WriteLine($"Verifying block at height {blockToVerify.Height}");

            if (blockToVerify.State != BlockState.Mined)
            {
                throw new Exception($"Invalid block state at height: {blockToVerify.Height} with hash: {blockToVerify.Hash}");
            }

            if (blockToVerify.Parent?.Hash != blockToVerify.ParentHash)
            {
                throw new Exception($"Invalid parent hash at height: {blockToVerify.Height}. Expected {blockToVerify.ParentHash}, got {blockToVerify.Parent?.Hash}");
            }

            if (!blockToVerify.Hash.StartsWith(new string(BufferCharacter, blockToVerify.Difficulty)))
            {
                throw new Exception($"Block format incorrect. Does not start with {blockToVerify.Difficulty} characters");
            }

            return blockToVerify.Height == 0 || HashBlock(blockToVerify) == blockToVerify.Hash;
        }

        private static string HashBlock(Block blockToHash)
        {
            return CalculateHash(blockToHash.Nonce, blockToHash.Height, blockToHash.Parent,
                blockToHash.RecordedTransaction, blockToHash.MinedDate, blockToHash.IterationsToMinedResult,
                blockToHash.Difficulty);
        }

        private static string CalculateHash(string nonce, long height, Block parent, ITransaction recordedTransaction, DateTimeOffset minedDate, int iterationsToMinedResult, int difficulty)
        {
            var blockString = nonce + height + parent?.Hash +
                              recordedTransaction + minedDate.UtcTicks +
                              iterationsToMinedResult + difficulty;

            var byteEncodedString = Encoding.UTF8.GetBytes(blockString);
            using (var hasher = SHA256.Create())
            {
                var hash = hasher.ComputeHash(byteEncodedString);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

    }
}
