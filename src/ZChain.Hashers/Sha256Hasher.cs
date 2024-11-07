using System.Security.Cryptography;
using System.Text;
using ZChain.Core;

namespace ZChain.Hashers;

public class Sha256Hasher : IHasher
{
    [ThreadStatic]
    private static SHA256? _hasher;

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

    public string ComputeHash(string input)
    {
        var byteEncodedString = Encoding.UTF8.GetBytes(input);
        var hash = Hasher.ComputeHash(byteEncodedString);
        return BitConverter.ToString(hash).Replace("-", "");
    }
}
