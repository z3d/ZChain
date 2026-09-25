using System.Buffers.Binary;
using System.Runtime.Intrinsics;

namespace ZChain.Hashers;

// SHA-256 over 8 independent messages at once, one per Vector256<uint> lane.
// When all 8 messages share their leading whole 64-byte blocks (mining: same block bytes, different nonce at the end),
// the state after those blocks is computed once and cached per thread, so each message only compresses its tail.
internal static class Sha256Vector8
{
    public const int Lanes = 8;
    private const int BlockSize = 64;
    private const int StateWords = 8;

    private static readonly uint[] RoundConstants =
    [
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
    ];

    private static readonly uint[] InitialState =
    [
        0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19,
    ];

    [ThreadStatic]
    private static byte[]? _cachedPrefix;

    [ThreadStatic]
    private static Vector256<uint>[]? _cachedMidstate;

    public static bool IsSupported => Vector256.IsHardwareAccelerated;

    // Hashes 8 equal-length messages stored back to back in inputs, writing 8 digests of 32 bytes back to back into digests.
    public static void HashLanes(ReadOnlySpan<byte> inputs, int inputLength, Span<byte> digests)
    {
        Span<Vector256<uint>> state = stackalloc Vector256<uint>[StateWords];
        int prefixLength = SharedPrefixLength(inputs, inputLength);
        if (prefixLength > 0)
        {
            LoadMidstate(inputs[..prefixLength], state);
        }
        else
        {
            for (int i = 0; i < StateWords; i++)
            {
                state[i] = Vector256.Create(InitialState[i]);
            }
        }

        int tailLength = inputLength - prefixLength;
        int blockCount = (tailLength + 9 + BlockSize - 1) / BlockSize;
        int paddedLength = blockCount * BlockSize;
        Span<byte> padded = paddedLength * Lanes <= 2048 ? stackalloc byte[paddedLength * Lanes] : new byte[paddedLength * Lanes];
        padded.Clear();
        for (int lane = 0; lane < Lanes; lane++)
        {
            Span<byte> message = padded.Slice(lane * paddedLength, paddedLength);
            inputs.Slice(lane * inputLength + prefixLength, tailLength).CopyTo(message);
            message[tailLength] = 0x80;
            BinaryPrimitives.WriteUInt64BigEndian(message[^8..], (ulong)inputLength * 8);
        }

        Span<Vector256<uint>> words = stackalloc Vector256<uint>[16];
        for (int block = 0; block < blockCount; block++)
        {
            for (int t = 0; t < 16; t++)
            {
                int offset = block * BlockSize + t * 4;
                words[t] = Vector256.Create(
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(0 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(1 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(2 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(3 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(4 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(5 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(6 * paddedLength + offset)..]),
                    BinaryPrimitives.ReadUInt32BigEndian(padded[(7 * paddedLength + offset)..]));
            }

            Compress(state, words);
        }

        for (int lane = 0; lane < Lanes; lane++)
        {
            Span<byte> digest = digests.Slice(lane * 32, 32);
            for (int i = 0; i < StateWords; i++)
            {
                BinaryPrimitives.WriteUInt32BigEndian(digest[(i * 4)..], state[i].GetElement(lane));
            }
        }
    }

    // Length of the leading whole blocks that every lane has in common, or 0.
    private static int SharedPrefixLength(ReadOnlySpan<byte> inputs, int inputLength)
    {
        int prefixLength = inputLength / BlockSize * BlockSize;
        ReadOnlySpan<byte> first = inputs[..prefixLength];
        for (int lane = 1; lane < Lanes; lane++)
        {
            if (!inputs.Slice(lane * inputLength, prefixLength).SequenceEqual(first))
            {
                return 0;
            }
        }

        return prefixLength;
    }

    private static void LoadMidstate(ReadOnlySpan<byte> prefix, Span<Vector256<uint>> state)
    {
        if (_cachedPrefix is not null && _cachedMidstate is not null && prefix.SequenceEqual(_cachedPrefix))
        {
            _cachedMidstate.CopyTo(state);
            return;
        }

        for (int i = 0; i < StateWords; i++)
        {
            state[i] = Vector256.Create(InitialState[i]);
        }

        Span<Vector256<uint>> words = stackalloc Vector256<uint>[16];
        for (int offset = 0; offset < prefix.Length; offset += BlockSize)
        {
            for (int t = 0; t < 16; t++)
            {
                words[t] = Vector256.Create(BinaryPrimitives.ReadUInt32BigEndian(prefix[(offset + t * 4)..]));
            }

            Compress(state, words);
        }

        _cachedPrefix = prefix.ToArray();
        _cachedMidstate = state.ToArray();
    }

    private static void Compress(Span<Vector256<uint>> state, ReadOnlySpan<Vector256<uint>> words)
    {
        Span<Vector256<uint>> schedule = stackalloc Vector256<uint>[64];
        words.CopyTo(schedule);
        for (int t = 16; t < 64; t++)
        {
            schedule[t] = SmallSigma1(schedule[t - 2]) + schedule[t - 7] + SmallSigma0(schedule[t - 15]) + schedule[t - 16];
        }

        Vector256<uint> a = state[0], b = state[1], c = state[2], d = state[3], e = state[4], f = state[5], g = state[6], h = state[7];
        for (int t = 0; t < 64; t++)
        {
            Vector256<uint> t1 = h + BigSigma1(e) + Choose(e, f, g) + Vector256.Create(RoundConstants[t]) + schedule[t];
            Vector256<uint> t2 = BigSigma0(a) + Majority(a, b, c);
            h = g;
            g = f;
            f = e;
            e = d + t1;
            d = c;
            c = b;
            b = a;
            a = t1 + t2;
        }

        state[0] += a;
        state[1] += b;
        state[2] += c;
        state[3] += d;
        state[4] += e;
        state[5] += f;
        state[6] += g;
        state[7] += h;
    }

    private static Vector256<uint> RotateRight(Vector256<uint> x, int bits) => (x >>> bits) | (x << (32 - bits));

    private static Vector256<uint> Choose(Vector256<uint> e, Vector256<uint> f, Vector256<uint> g) => (e & f) ^ Vector256.AndNot(g, e);

    private static Vector256<uint> Majority(Vector256<uint> a, Vector256<uint> b, Vector256<uint> c) => (a & b) ^ (a & c) ^ (b & c);

    private static Vector256<uint> BigSigma0(Vector256<uint> x) => RotateRight(x, 2) ^ RotateRight(x, 13) ^ RotateRight(x, 22);

    private static Vector256<uint> BigSigma1(Vector256<uint> x) => RotateRight(x, 6) ^ RotateRight(x, 11) ^ RotateRight(x, 25);

    private static Vector256<uint> SmallSigma0(Vector256<uint> x) => RotateRight(x, 7) ^ RotateRight(x, 18) ^ (x >>> 3);

    private static Vector256<uint> SmallSigma1(Vector256<uint> x) => RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >>> 10);
}
