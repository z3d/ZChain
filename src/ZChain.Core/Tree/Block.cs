using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ZChain.Core.Tree
{
    public class Block<T>
    {
        public const char DefaultBufferCharacter = '0';

        public Block<T> Parent { get; private set; }
        public T RecordedTransaction { get; private set; }
        public int Difficulty { get; private set; }
        public BlockState State { get; private set; }
        public string Hash { get; private set; }
        public string ParentHash { get; private set; }
        public string Nonce { get; private set; }

        public DateTimeOffset BeginMiningDate { get; private set; }

        public long Height { get; private set; }

        public static Block<T> CreateGenesisBlock(T recordedTransaction)
        {
            return new Block<T>
            {
                Hash = new string(DefaultBufferCharacter, 32),
                Parent = null,
                Height = 0,
                RecordedTransaction = recordedTransaction,
                State = BlockState.Mined,
                BeginMiningDate = DateTimeOffset.Now
            };
        }

        private Block()
        {                
        }

        public void SetMiningBeginning()
        {
            if (State != BlockState.New)
            {
                throw new Exception("Cannot remine a block");
            }
            BeginMiningDate = DateTimeOffset.Now;
            State = BlockState.Mining;
        }

        public void SetMinedValues(string nonce, string hash)
        {
            if (State != BlockState.Mining)
            {
                throw new Exception("Cannot set state of a block that isn't being mined");
            }

           Nonce = nonce;
           Hash = hash;
           State = BlockState.Mined;
           Verify();
        }

        public Block(Block<T> parent, T recordedTransaction, int difficulty): this(recordedTransaction, difficulty)
        {
            Parent = parent ?? throw new ArgumentNullException(
                         $"Parent of block cannot be null. Create genesis block using factory method and use as the root.");
            ParentHash = parent.Hash;
            Height = parent.Height + 1;
        }

        private Block(T recordedTransaction, int difficulty)
        {
            if(difficulty <= 0)
            {
                throw new Exception("Difficulty must exceed 0");
            }

            RecordedTransaction = recordedTransaction;
            Difficulty = difficulty;
            State = BlockState.New;
            Hash = "NEW_BLOCK";
        }
   
        public override string ToString()
        {
            return
                $"Hash: {Hash} Parent Hash: {Parent?.Hash} Height: {Height} Transaction: {RecordedTransaction} " +
                $"Nonce: {Nonce} Difficulty: {Difficulty} Received Date: {BeginMiningDate}";
        }

        public static string CalculateHash(string nonce, long height, Block<T> parent, T recordedTransaction, int difficulty)
        {
            var blockString = nonce + height + parent?.Hash +
                              recordedTransaction +
                              difficulty;

            var byteEncodedString = Encoding.UTF8.GetBytes(blockString);
            using (var hasher = SHA256.Create())
            {
                var hash = hasher.ComputeHash(byteEncodedString);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public bool Verify(char bufferCharacter = DefaultBufferCharacter)
        {
            string HashBlock()
            {
                return CalculateHash(Nonce, Height, Parent,
                    RecordedTransaction,
                    Difficulty);
            }

            if (Height != 0)
            {
                Parent.Verify(bufferCharacter); // Recursively verify chain
            }
            else if (Hash != new string(bufferCharacter, 32))
            {
                throw new Exception($"Genesis block hash incorrect");
            }

            Debug.WriteLine($"Verifying block at height {Height}");

            if (State != BlockState.Mined)
            {
                throw new Exception($"Invalid block state, of {State} at height: {Height} with hash: {Hash}");
            }

            if (Parent?.Hash != ParentHash)
            {
                throw new Exception($"Invalid parent hash at height: {Height}. Expected {ParentHash}, got {Parent?.Hash}");
            }

            if (!Hash.StartsWith(new string(bufferCharacter, Difficulty)))
            {
                throw new Exception($"Block format incorrect. Does not start with {Difficulty} characters");
            }

            return Height == 0 || HashBlock() == Hash;
        }

    }
}
