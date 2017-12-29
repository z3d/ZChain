using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ZChain.Core.Tree
{
    public class Block
    {
        private static readonly char BufferCharacter = '0';

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

        public void MineBlock()
        {
            if (State != BlockState.New)
            {
                throw new Exception("Cannot remine a block");
            }

            State = BlockState.Mining;
            
            var targetHashStart = new string(BufferCharacter, Difficulty);
            ReceivedDate = DateTimeOffset.Now;
            while (!Hash.StartsWith(targetHashStart))
            {
                ++IterationsToMinedResult;
                Nonce = GenerateNonce();
                MinedDate = DateTimeOffset.Now;
                Hash = HashBlock(this);
            }
            State = BlockState.Mined;
            Verify(this);
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
            else
            {
                if (blockToVerify.Hash != new string(BufferCharacter, 32))
                {
                    throw new Exception($"Genesis block hash incorrect");
                }
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
            var blockString = blockToHash.Nonce + blockToHash.Height + blockToHash.Parent?.Hash +
                        blockToHash.RecordedTransaction + blockToHash.MinedDate.UtcTicks +
                        blockToHash.IterationsToMinedResult + blockToHash.Difficulty;

            var byteEncodedString = Encoding.UTF8.GetBytes(blockString);
            using (var hasher = SHA256.Create())
            {
                var hash = hasher.ComputeHash(byteEncodedString);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
