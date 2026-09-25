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

    public void ComputeHashes(ReadOnlySpan<byte> inputs, int inputLength, Span<byte> destinations)
    {
        if (Sha256Vector8.IsSupported && inputs.Length == inputLength * Sha256Vector8.Lanes)
        {
            Sha256Vector8.HashLanes(inputs, inputLength, destinations);
            return;
        }

        int count = inputs.Length / inputLength;
        for (int i = 0; i < count; i++)
        {
            ComputeHash(inputs.Slice(i * inputLength, inputLength), destinations.Slice(i * HashSizeInBytes, HashSizeInBytes));
        }
    }
}
