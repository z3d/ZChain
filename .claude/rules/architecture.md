# Architecture Guide

## Project Dependencies

```
                    ┌─────────────────┐
                    │   ZChain.Core   │
                    │  (Domain Layer) │
                    └────────┬────────┘
                             │
            ┌────────────────┼────────────────┐
            │                │                │
            ▼                ▼                ▼
    ┌───────────────┐ ┌─────────────┐ ┌───────────────────┐
    │ ZChain.Hashers│ │ZChain.CpuMiner│ │   (Future miners)  │
    │   (SHA256)    │ │  (PoW impl) │ │   GPU, ASIC, etc   │
    └───────────────┘ └─────────────┘ └───────────────────┘
            │                │
            └────────┬───────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
    ▼                ▼                ▼
┌─────────┐  ┌──────────────┐  ┌─────────────────────┐
│  Tests  │  │  ConsoleApp  │  │ PerformanceTesting  │
└─────────┘  └──────────────┘  └─────────────────────┘
```

## Core Domain Model

### Block<T> State Machine

```
    ┌─────┐
    │ New │ ──────────────────────────────────────┐
    └──┬──┘                                       │
       │ BeginMining()                            │
       ▼                                          │
  ┌────────┐                                      │
  │ Mining │ ◄────────────────────────────────────┤
  └───┬────┘                                      │
      │ SetMinedValues(hash, nonce)               │
      ▼                                           │
  ┌───────┐                                       │
  │ Mined │                                       │
  └───┬───┘                                       │
      │ Verify() - validates hash matches         │
      │            recorded transaction           │
      ▼                                           │
  [Verification passes or throws]                 │
                                                  │
  Invalid transitions throw BlockStateException ◄─┘
```

### Generic Transaction Design

```csharp
Block<T> where T : class
```

The generic parameter allows any transaction type:
- `Block<MoneyTransferTransaction>` - Financial transfers
- `Block<string>` - Simple data storage
- `Block<CustomPayload>` - Domain-specific data

### Builder Pattern Rationale

Builders enforce construction invariants at compile time:

```csharp
// Compile error: Build() requires WithHasher() to be called
var invalid = new BlockBuilder<Tx>()
    .WithTransaction(tx)
    .Build();  // Won't compile without hasher

// Correct usage
var valid = new BlockBuilder<Tx>()
    .WithTransaction(tx)
    .WithHasher(new Sha256Hasher())
    .Build();
```

## Mining Architecture

### Multi-threaded Mining Strategy

```
Main Thread
    │
    ├── Creates CancellationTokenSource
    │
    ├── Spawns N worker tasks
    │   │
    │   ├── Worker 1: Loop { generate nonce, hash, check difficulty }
    │   ├── Worker 2: Loop { generate nonce, hash, check difficulty }
    │   └── Worker N: Loop { generate nonce, hash, check difficulty }
    │
    ├── Task.WhenAny() - waits for first success
    │
    ├── Cancels remaining workers
    │
    └── Returns winning hash/nonce
```

### Difficulty Implementation

Difficulty = number of leading zeros required in hash:
- Difficulty 1: Hash starts with "0"
- Difficulty 2: Hash starts with "00"
- Difficulty 3: Hash starts with "000"

Higher difficulty = exponentially more work (16x per level on average).

## Extension Points

### Adding New Hasher

1. Create class implementing `IHasher` in `ZChain.Hashers`
2. Implement `ComputeHash(string input): string`
3. Consider thread safety (use thread-static or instance per call)

```csharp
public class Sha3Hasher : IHasher
{
    public string ComputeHash(string input)
    {
        // Implementation
    }
}
```

### Adding New Miner

1. Create class implementing `IMiner<T>` in new project or `ZChain.CpuMiner`
2. Reference `ZChain.Core`
3. Implement async `MineBlock(Block<T> block): Task`

```csharp
public class GpuMiner<T> : IMiner<T> where T : class
{
    public async Task MineBlock(Block<T> block)
    {
        // GPU-accelerated mining
    }
}
```

### Adding New Transaction Type

1. Create class/record in `ZChain.Core` or consuming project
2. Use with `Block<YourTransaction>`

```csharp
public record NftTransfer(string TokenId, string From, string To);

var block = new BlockBuilder<NftTransfer>()
    .WithTransaction(new NftTransfer("token1", "alice", "bob"))
    .WithHasher(hasher)
    .Build();
```

## Performance Considerations

### Hashing

- `Sha256Hasher` uses thread-static instance to avoid allocation per hash
- Each hash operation: ~microseconds
- Mining difficulty 3 requires ~4000 hashes on average

### Memory

- Blocks hold reference to parent (linked list structure)
- Deep blockchains may need pagination for verification
- Consider memory-mapped approaches for production

### Threading

- Optimal thread count often equals CPU core count
- More threads than cores adds overhead without benefit
- Benchmark with your specific hardware to find sweet spot
