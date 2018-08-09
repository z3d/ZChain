using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ZChain.Core.Tree
{
    public static class BlockExtensions
    {
        private static string HashBlock(this Block blockToHash)
        {
            return CalculateHash(blockToHash.Nonce, blockToHash.Height, blockToHash.Parent,
                blockToHash.RecordedTransaction, blockToHash.MinedDate, blockToHash.IterationsToMinedResult,
                blockToHash.Difficulty);
        }

        public static string CalculateHash(string nonce, long height, Block parent, ITransaction recordedTransaction, DateTimeOffset minedDate, int iterationsToMinedResult, int difficulty)
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

        public static bool Verify(this Block blockToVerify, char bufferCharacter = Block.DefaultBufferCharacter)
        {
            if (blockToVerify.Height != 0)
            {
                blockToVerify.Parent.Verify(bufferCharacter); // Recursively verify chain
            }
            else if (blockToVerify.Hash != new string(bufferCharacter, 32))
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

            if (!blockToVerify.Hash.StartsWith(new string(bufferCharacter, blockToVerify.Difficulty)))
            {
                throw new Exception($"Block format incorrect. Does not start with {blockToVerify.Difficulty} characters");
            }

            return blockToVerify.Height == 0 || HashBlock(blockToVerify) == blockToVerify.Hash;
        }
    }
}
