using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ZChain.Core.Tree
{
    public class Block
    {
        public static Block CreateGenesisBlock(ITransaction recordedTransaction, int difficulty)
        {
            var genesisBlock = new Block(recordedTransaction, difficulty)
            {
                Hash = new string('0', 32),
                Parent = null,
                Height = 0
            };

            return genesisBlock;
        }

        public Block(Block parent, ITransaction recordedTransaction, int difficulty): this(recordedTransaction, difficulty)
        {
            Parent = parent;
            ParentHash = parent.Hash;
            Height = parent.Height + 1;
        }

        private Block(ITransaction recordedTransaction, int difficulty)
        {
            if(difficulty <= 0)
            {
                throw new Exception("Difficulty must exceed 1");
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

        public void MineBlock()
        {
            if (State != BlockState.New)
            {
                throw new Exception("Cannot remine a block");
            }

            State = BlockState.Mining;
            
            var hashStart = new string('0', Difficulty);
            ReceivedDate = DateTimeOffset.Now;
            while (!Hash.StartsWith(hashStart))
            {
                ++IterationsToMinedResult;
                Nonce = GenerateNonce();
                MinedDate = DateTimeOffset.Now;
                Hash = HashBlock(this);
            }
            State = BlockState.Mined;
        }

        public override string ToString()
        {
            return
                $"Hash: {Hash} Parent Hash: {Parent?.Hash} Height: {Height} Transaction: {RecordedTransaction} Nonce: {Nonce} Difficulty: {Difficulty} Received Date: {ReceivedDate} Mined Date: {MinedDate} Iterations to mine Result: {IterationsToMinedResult}";
        }

        private string GenerateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static bool Verify(Block blockToVerify)
        {
            if (blockToVerify.Height != 0)
            {
                Verify(blockToVerify.Parent);
            }
            Debug.WriteLine($"Verifying block at height {blockToVerify.Height}");

            if (blockToVerify.State != BlockState.Mined)
            {
                throw new Exception($"Invalid block state at height: {blockToVerify.Height} with hash: {blockToVerify.Hash}");
            }

            if (blockToVerify.Parent?.Hash != blockToVerify.ParentHash)
            {
                throw new Exception($"Invalid parent hash at height: {blockToVerify.Height}. Expected {blockToVerify.ParentHash}, got {blockToVerify.Parent.Hash}");
            }

            return blockToVerify.Height == 0 || HashBlock(blockToVerify) == blockToVerify.Hash;
        }

        private static string HashBlock(Block blockToHash)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(blockToHash.Nonce);
            stringBuilder.Append(blockToHash.Height);
            stringBuilder.Append(blockToHash.Parent?.Hash);
            stringBuilder.Append(blockToHash.RecordedTransaction);
            stringBuilder.Append(blockToHash.MinedDate.UtcTicks);
            stringBuilder.Append(blockToHash.IterationsToMinedResult);

            var byteEncodedString = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            using (var hasher = SHA256.Create())
            {
                var hash = hasher.ComputeHash(byteEncodedString);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
