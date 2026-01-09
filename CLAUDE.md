# ZChain

A .NET-based blockchain implementation with Proof-of-Work mining, built for learning and performance experimentation.

## Quick Reference

```bash
dotnet format src/ZChain.sln           # Format code
dotnet build src/ZChain.sln            # Build (warnings = errors)
dotnet test src/ZChain.sln             # Run all tests
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj  # Benchmarks
```

## Pre-Commit Checklist

1. `dotnet format src/ZChain.sln` - Apply formatting
2. `dotnet build src/ZChain.sln` - Verify compilation
3. `dotnet test src/ZChain.sln` - All tests must pass

## Architecture

```
                    ┌─────────────────┐
                    │   ZChain.Core   │
                    │  (Domain Layer) │
                    └────────┬────────┘
                             │
            ┌────────────────┼────────────────┐
            ▼                ▼                ▼
    ┌───────────────┐ ┌─────────────┐ ┌───────────────────┐
    │ ZChain.Hashers│ │ZChain.CpuMiner│ │   (Future miners)  │
    └───────────────┘ └─────────────┘ └───────────────────┘
            │                │
            └────────┬───────┘
                     │
    ┌────────────────┼────────────────┐
    ▼                ▼                ▼
┌─────────┐  ┌──────────────┐  ┌─────────────────────┐
│  Tests  │  │  ConsoleApp  │  │ PerformanceTesting  │
└─────────┘  └──────────────┘  └─────────────────────┘
```

### Block<T> State Machine

```
    ┌─────┐
    │ New │ ───────────────────────────────────────┐
    └──┬──┘                                        │
       │ BeginMining()                             │
       ▼                                           │
  ┌────────┐                                       │
  │ Mining │                                       │
  └───┬────┘                                       │
      │ SetMinedValues(hash, nonce)                │
      ▼                                            │
  ┌───────┐                                        │
  │ Mined │ ───► Verify() validates integrity     │
  └───────┘                                        │
                                                   │
  Invalid transitions throw BlockStateException ◄──┘
```

### Core Components

- **Block<T>**: Generic block with state machine (New → Mining → Mined)
- **CpuMiner<T>**: Multi-threaded PoW miner with cancellation support
- **BlockBuilder<T>**: Fluent API enforcing required dependencies at compile time
- **IHasher/IMiner<T>**: Abstractions for hashing and mining strategies

## Code Style

Enforced via `.editorconfig`. Key rules:

- **Line length**: 180 chars max
- **Namespaces**: File-scoped required (`namespace Foo;`)
- **Security**: Analyzer diagnostics as errors

### Prohibited

- ❌ `#region` blocks - use smaller classes
- ❌ Commented-out code - use git history
- ❌ XML doc comments - self-documenting code
- ❌ `var` when type isn't obvious

### Required

- ✅ File-scoped namespaces
- ✅ Primary constructors for simple DI
- ✅ Pattern matching over type checks
- ✅ `ArgumentNullException.ThrowIfNull()` for validation

### var Usage

```csharp
// GOOD: Type is obvious
var items = new List<string>();
var block = new Block<Transaction>();

// BAD: Use explicit type
string result = GetResult();
IHasher hasher = CreateHasher();
```

## Testing

Framework: xUnit + Shouldly | Location: `src/ZChain.Tests/`

### Naming Convention

```csharp
[Fact]
public void WhenCondition_AndContext_ShouldExpectedBehavior()
```

### Structure (Arrange-Act-Assert)

```csharp
[Fact]
public void WhenCreatingBlock_WithValidData_ShouldSucceed()
{
    // Arrange
    var hasher = new StubHasher();
    var transaction = new MoneyTransferTransaction("A", "B", 100);

    // Act
    var block = new BlockBuilder<MoneyTransferTransaction>()
        .WithTransaction(transaction)
        .WithHasher(hasher)
        .Build();

    // Assert
    block.State.ShouldBe(BlockState.New);
}
```

### Assertions

```csharp
// Prefer Shouldly
block.State.ShouldBe(BlockState.Mined);
block.Hash.ShouldStartWith("0000");
Should.Throw<BlockStateException>(() => block.Mine());
```

### Stubs Over Mocks

Use `StubHasher`, `StubMiner<T>` - avoid mocking frameworks.

## Extension Points

### Adding New Hasher

```csharp
// In ZChain.Hashers
public class Sha3Hasher : IHasher
{
    public string ComputeHash(string input) { /* impl */ }
}
```

### Adding New Miner

```csharp
// In ZChain.CpuMiner or new project
public class GpuMiner<T> : IMiner<T> where T : class
{
    public async Task MineBlock(Block<T> block) { /* impl */ }
}
```

### Adding New Transaction Type

```csharp
public record NftTransfer(string TokenId, string From, string To);

var block = new BlockBuilder<NftTransfer>()
    .WithTransaction(new NftTransfer("token1", "alice", "bob"))
    .WithHasher(hasher)
    .Build();
```

## Git Workflow

### Branch Naming

```
feature/description    # New functionality
fix/description        # Bug fixes
chore/description      # Dependencies, config
```

### Commit Format

```
Short summary in imperative mood (50 chars)

Optional body explaining "why" not "what".

Co-Authored-By: Claude <noreply@anthropic.com>
```

### PR Template

```markdown
## Summary
- Change 1
- Change 2

## Test plan
- [x] Unit tests pass
- [x] Integration tests pass

Generated with [Claude Code](https://claude.com/claude-code)
```

### Common Operations

```bash
# Start new work
git checkout main && git pull
git checkout -b feature/my-feature

# Create PR
gh pr create --title "Add feature" --body "## Summary..."

# After merge
git checkout main && git pull
git branch -d feature/my-feature
```

## Performance Benchmarking

BenchmarkDotNet 0.14.0 (pinned - 0.15.x showed high variance)

- **ThreadCount**: 1, 2, 3, 10
- **Difficulty**: 1, 2, 3 (leading zeros)
- **Results**: `BenchmarkDotNet.Artifacts/results/`
- **Healthy variance**: <25%

## Dependencies

| Package | Purpose |
|---------|---------|
| Newtonsoft.Json | Block serialization |
| BenchmarkDotNet 0.14.0 | Performance measurement |
| xunit + Shouldly | Testing |

## Files to Never Commit

- `BenchmarkDotNet.Artifacts/` - Machine-specific
- `*.user`, `.vs/`, `bin/`, `obj/`

## Repository Config

```bash
git config user.name "z3d"
git config user.email "925699+z3d@users.noreply.github.com"
```
