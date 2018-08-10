using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public interface IMiner
    {
        Task<Block> MineBlock();
    }
}