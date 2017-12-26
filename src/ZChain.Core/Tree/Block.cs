using System;
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
                Level = 0
            };

            return genesisBlock;
        }

        public Block(Block parent, ITransaction recordedTransaction, int difficulty): this(recordedTransaction, difficulty)
        {
            Parent = parent;
            Level = parent.Level + 1;
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
        }

        public Block Parent { get; private set; }
        public ITransaction RecordedTransaction { get; private set; }
        public int Difficulty { get; private set; }
        public BlockState State { get; private set; }

        public string Hash { get; private set; }

        public string Nonce { get; private set; }

        public DateTimeOffset MinedDate { get; private set; }

        public long Level { get; private set; }


        public void MineBlock()
        {
            State = BlockState.Mining;
            var hashStart = new string('0', Difficulty);

            while (!Hash.StartsWith(hashStart))
            {
                Nonce = GenerateNonce();
                Hash = HashBlock(this);
            }

            MinedDate = DateTimeOffset.Now;
            State = BlockState.Mined;
        }

        public override string ToString()
        {
            return
                $"Hash: {Hash} Parent Hash: {Parent?.Hash} Level: {Level} Transaction: {RecordedTransaction} Nonce: {Nonce} Difficulty: {Difficulty} Mined Date: {MinedDate}";
        }

        private string GenerateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static bool VerifyHash(Block blockToVerify)
        {
            return HashBlock(blockToVerify) == blockToVerify.Hash;
        }

        private static string HashBlock(Block blockToHash)
        {
            var combinedString = blockToHash.Nonce + blockToHash.Level + blockToHash.Parent?.Hash + blockToHash.RecordedTransaction;

            var byteEncodedString = Encoding.UTF8.GetBytes(combinedString);
            var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(byteEncodedString);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
