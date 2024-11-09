using System;

namespace ZChain.Core;

public class BlockStateException(string message) : Exception(message)
{
}
