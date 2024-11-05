using System;

namespace ZChain.Core;

public class BlockStateException : Exception
{
    public BlockStateException(string message): base(message)
    {
       
    }        
}
