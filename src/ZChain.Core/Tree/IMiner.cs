using System.Threading.Tasks;

namespace ZChain.Core.Tree
{
    public interface IMiner<T>
    {
        Task MineBlock(Block<T> blockToMine);
    }
}