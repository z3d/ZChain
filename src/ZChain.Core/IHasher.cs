using System;

namespace ZChain.Core;

public interface IHasher
{
    int HashSizeInBytes { get; }

    void ComputeHash(ReadOnlySpan<byte> input, Span<byte> destination);

    // Hashes equal-length inputs stored back to back, writing digests back to back. Implementations may batch.
    void ComputeHashes(ReadOnlySpan<byte> inputs, int inputLength, Span<byte> destinations)
    {
        int count = inputs.Length / inputLength;
        for (int i = 0; i < count; i++)
        {
            ComputeHash(inputs.Slice(i * inputLength, inputLength), destinations.Slice(i * HashSizeInBytes, HashSizeInBytes));
        }
    }
}
