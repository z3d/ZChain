using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZChain.Core;

namespace ZChain.Tests.UnitTests.Domain.BlockTests
{
    public class BlockTests
    {
        private readonly Block<MoneyTransferDummyTransaction> _firstChild;
        private readonly MoneyTransferDummyTransaction _moneyTransferDummyTransaction = new MoneyTransferDummyTransaction("Second_Address", "Third_Address", 200);

        class StubMiner<T> : IMiner<T>
        {
            private readonly string _nonce;
            private readonly string _hash;

            public StubMiner(string nonce, string hash)
            {
                _nonce = nonce;
                _hash = hash;
            }

            public Task MineBlock(Block<T> blockToMine)
            {
                blockToMine.SetMiningBeginning();
                blockToMine.SetMinedValues(_nonce, _hash);
                return Task.CompletedTask;
            }
        }

        public BlockTests()
        {
            var rootBlock = new Block<MoneyTransferDummyTransaction>(null, new MoneyTransferDummyTransaction("First_Address", "Second_Address",300), 4);
            var miner = new StubMiner<MoneyTransferDummyTransaction>("d31d42f5137b4f32ae8b78875d8e46e1", "0000E4898D26DB1BAF66E6AD187F5573117946C14F4021046461DCF0EDADE1EC");
            miner.MineBlock(rootBlock);
            _firstChild = new Block<MoneyTransferDummyTransaction>(rootBlock, _moneyTransferDummyTransaction, 4);
        }
        
        [Fact]
        public void WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow()
        {
            var miner = new StubMiner<MoneyTransferDummyTransaction>("a80a69ff16b94b3784bb200db2638b91", "0000FFF686BF9624BACCF8CB9E6F02B96450FF8C844A043C4072719D007833C4");
            miner.MineBlock(_firstChild);

            Should.Throw<InvalidOperationException>(() => miner.MineBlock(_firstChild));
        }

        [Fact]
        public void WhenMiningABlock_AndSettingABadNonce_ShouldThrow()
        {
            var miner = new StubMiner<MoneyTransferDummyTransaction>("31e359e12e9a4ad19559fbffe8f3c15a", "0041AD4CC3B9C508336BA949D302076E0A47DAB769B7D2A6F216791A913A4036");
            Should.Throw<BlockStateException>(() => miner.MineBlock(_firstChild));
        }

        [Fact]
        public void WhenHavingANewBlock_AndAttemptingToVerify_ShouldThrow()
        {
            var rootBlock = new Block<MoneyTransferDummyTransaction>(null, _moneyTransferDummyTransaction, 2);

            var newBlock = new Block<MoneyTransferDummyTransaction>(rootBlock, _moneyTransferDummyTransaction, 2);

            Should.Throw<BlockStateException>(() => newBlock.VerifyMinedBlock());
        }

        [Fact]
        public void WhenMiningABlockAndSettingValues_AndTheHashDoesntMatchTheExpectedDifficulty_Throws()
        {
            var rootBlock = new Block<MoneyTransferDummyTransaction>(null, _moneyTransferDummyTransaction, 2);
            var newBlock = new Block<MoneyTransferDummyTransaction>(rootBlock, _moneyTransferDummyTransaction, 2);

            var miner = new StubMiner<MoneyTransferDummyTransaction>("5b890bf59d1b421aa7e02b6fba2524be", "05EEC80E461F2908F7A6070990699F233383539798E5BAD3F94DBD9096554F5D");
            Should.Throw<BlockStateException>(() => miner.MineBlock(newBlock));
        }
    }
}
