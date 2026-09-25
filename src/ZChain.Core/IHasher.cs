using System;

namespace ZChain.Core;

public interface IHasher
{
    int HashSizeInBytes { get; }
    void ComputeHash(ReadOnlySpan<byte> input, Span<byte> destination);
}
