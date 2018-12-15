using System.Threading.Tasks;

namespace ZChain.Core
{
    public interface IMiner<T>
    {
        Task MineBlock(Block<T> blockToMine);
    }
}