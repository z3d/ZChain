using System;
using System.Security.Cryptography;
using Shouldly;
using Xunit;
using ZChain.Hashers;

namespace ZChain.Tests.UnitTests.Hashers;

public class Sha256HasherTests
{
    private readonly Sha256Hasher _hasher = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(55)]
    [InlineData(56)]
    [InlineData(64)]
    [InlineData(106)]
    [InlineData(119)]
    [InlineData(120)]
    [InlineData(200)]
    public void WhenHashingEightUnrelatedInputs_ShouldMatchSha256ForEveryLane(int inputLength)
    {
        // Arrange
        byte[] inputs = new byte[inputLength * 8];
        new Random(inputLength).NextBytes(inputs);
        byte[] digests = new byte[32 * 8];

        // Act
        _hasher.ComputeHashes(inputs, inputLength, digests);

        // Assert
        for (int lane = 0; lane < 8; lane++)
        {
            byte[] expected = SHA256.HashData(inputs.AsSpan(lane * inputLength, inputLength));
            digests.AsSpan(lane * 32, 32).ToArray().ShouldBe(expected, $"lane {lane}");
        }
    }

    [Theory]
    [InlineData(64, 16)]
    [InlineData(90, 16)]
    [InlineData(128, 1)]
    [InlineData(200, 40)]
    public void WhenHashingInputsThatShareAPrefix_ShouldMatchSha256AcrossRepeatedBatches(int prefixLength, int tailLength)
    {
        // Arrange
        int inputLength = prefixLength + tailLength;
        byte[] prefix = new byte[prefixLength];
        new Random(prefixLength).NextBytes(prefix);
        byte[] inputs = new byte[inputLength * 8];
        byte[] digests = new byte[32 * 8];

        // Act & Assert: the second batch hits the cached midstate, the third batch uses a different prefix
        for (int batch = 0; batch < 3; batch++)
        {
            if (batch == 2)
            {
                prefix[0] ^= 0xFF;
            }

            for (int lane = 0; lane < 8; lane++)
            {
                prefix.CopyTo(inputs, lane * inputLength);
                new Random(batch * 8 + lane).NextBytes(inputs.AsSpan(lane * inputLength + prefixLength, tailLength));
            }

            _hasher.ComputeHashes(inputs, inputLength, digests);

            for (int lane = 0; lane < 8; lane++)
            {
                byte[] expected = SHA256.HashData(inputs.AsSpan(lane * inputLength, inputLength));
                digests.AsSpan(lane * 32, 32).ToArray().ShouldBe(expected, $"batch {batch} lane {lane}");
            }
        }
    }
}
