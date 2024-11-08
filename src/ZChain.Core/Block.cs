﻿using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ZChain.Core;

public class Block<T>
{
    public const char DefaultBufferCharacter = '0';
    private readonly string _serializedTransaction;
    private readonly object _lockObject;
    private readonly string _blockstring;

    // See: https://stackoverflow.com/a/18086509/914352
    [ThreadStatic]
    // ReSharper disable once StaticMemberInGenericType
    private static SHA256 _hasher;

    private static SHA256 Hasher
    {
        get
        {
            if (_hasher == null)
            {
                _hasher = SHA256.Create();
            }
            return _hasher;
        }
    }

    public Block<T> Parent { get; }
    public T RecordedTransaction { get; }
    public int Difficulty { get; }
    public string ParentHash { get; }
    public long Height { get; }

    public BlockState State { get; private set; }
    public string Hash { get; private set; }
    public string Nonce { get; private set; }
    public DateTimeOffset BeginMiningDate { get; private set; }
  
    public Block(Block<T> parent, T recordedTransaction, int difficulty)
    {
        Parent = parent;
        ParentHash = parent?.Hash;
        Height = parent?.Height + 1 ?? 1;
       _serializedTransaction = JsonConvert.SerializeObject(recordedTransaction);
       _lockObject = new object();
        _blockstring = Height + Parent?.Hash + _serializedTransaction + Difficulty;

        if (difficulty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(difficulty), "Difficulty must exceed 0");
        }

        RecordedTransaction = recordedTransaction;
        Difficulty = difficulty;
        State = BlockState.New;
        Hash = "NEW_BLOCK";
    }        

    [JsonConstructor]
    // ReSharper disable once UnusedMember.Local
    private Block(Block<T> parent, T recordedTransaction, int difficulty, string nonce, string hash, DateTimeOffset beginMiningDate)
    {
        Parent = parent;
        RecordedTransaction = recordedTransaction;
        Difficulty = difficulty;
        Nonce = nonce;
        Hash = hash;
        BeginMiningDate = beginMiningDate;
    }

    public void SetMiningBeginning()
    {
        if (State != BlockState.New)
        {
            throw new InvalidOperationException("Cannot remine a block"); 
        }
        BeginMiningDate = DateTimeOffset.Now;
        State = BlockState.Mining;
    }

    public virtual void SetMinedValues(string nonce, string hash)
    {
        lock (_lockObject)
        {
            if (State != BlockState.Mining)
            {
                throw new InvalidOperationException("Cannot set mined values of a block that isn't being mined");
            }

            Nonce = nonce;
            Hash = hash;
            State = BlockState.Mined;
            if (!VerifyMinedBlock())
            {
                throw new Exception($"Could not set the mined values: {nameof(nonce)}: {nonce}. {nameof(hash)}: {hash}");
            }
        }
    }

    public override string ToString()
    {
        return
            $"Hash: {Hash} Parent Hash: {Parent?.Hash} Height: {Height} Transaction: {_serializedTransaction} " +
            $"Nonce: {Nonce} Difficulty: {Difficulty} Received Date: {BeginMiningDate}";
    }

    public string CalculateHash(string nonce)
    {
        var blockToHash = nonce + _blockstring;

        var byteEncodedString = Encoding.UTF8.GetBytes(blockToHash);

        var hash = Hasher.ComputeHash(byteEncodedString);
        return BitConverter.ToString(hash).Replace("-", "");

    }

    public string SerializeToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public bool VerifyMinedBlock(char bufferCharacter = DefaultBufferCharacter)
    {
        if (Height != 1)
        {
            Parent.VerifyMinedBlock(bufferCharacter); // Recursively verify chain
        }
       
        Debug.WriteLine($"Verifying block at height {Height}");

        if (State != BlockState.Mined)
        {
            throw new BlockStateException($"Invalid block state, of {State} at height: {Height} with hash: {Hash}");
        }

        if (Parent?.Hash != ParentHash)
        {
            throw new BlockStateException($"Invalid parent hash at height: {Height}. Expected {ParentHash}, got {Parent?.Hash}");
        }

        if (!Hash.StartsWith(new string(bufferCharacter, Difficulty)))
        {
            throw new BlockStateException($"Block format incorrect. Does not start with {Difficulty} characters");
        }

        if (Height == 0)
        {
            return true;
        }

        var calculatedHash = CalculateHash(Nonce);

        if (calculatedHash != Hash)
        {
            throw new BlockStateException($"Verification of block with {Hash} failed. Nonce was {Nonce}, and calculated Hash was {calculatedHash}");
        }

        return true;
    }

    public static Block<T> DeserializeBlockFromJsonString(string serialized)
    {
        return JsonConvert.DeserializeObject<Block<T>>(serialized);
    }
}
