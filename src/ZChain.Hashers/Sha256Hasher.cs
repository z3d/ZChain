using System.Security.Cryptography;
using ZChain.Core;

namespace ZChain.Hashers;

public class Sha256Hasher : IHasher
{
    // ponytail: a reused per-thread instance is ~40% faster than the one-shot SHA256.HashData on Windows CNG
    [ThreadStatic]
    private static SHA256? _hasher;

    public int HashSizeInBytes => SHA256.HashSizeInBytes;

    public void ComputeHash(ReadOnlySpan<byte> input, Span<byte> destination)
    {
        _hasher ??= SHA256.Create();
        if (!_hasher.TryComputeHash(input, destination, out _))
        {
            throw new ArgumentException($"Destination must hold at least {HashSizeInBytes} bytes", nameof(destination));
        }
    }
}
