---
name: new-feature
description: Guide for implementing new features in ZChain. Use when adding new functionality, creating new classes, extending the blockchain, or implementing new miners/hashers.
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
---

# Implementing New Features

Step-by-step guide for adding functionality to ZChain.

## Architecture Quick Reference

```
ZChain.Core        → Domain models, interfaces, builders
ZChain.CpuMiner    → Mining implementations (IMiner<T>)
ZChain.Hashers     → Hash algorithms (IHasher)
ZChain.Tests       → Unit + integration tests
```

## Feature Types

### Adding a New Transaction Type

1. Create record/class (can be in consuming project or Core):
```csharp
public record MyTransaction(string Field1, decimal Field2);
```

2. Use with existing Block<T>:
```csharp
var block = new BlockBuilder<MyTransaction>()
    .WithTransaction(new MyTransaction("value", 100m))
    .WithHasher(new Sha256Hasher())
    .Build();
```

### Adding a New Hasher

1. Create in `ZChain.Hashers`:
```csharp
namespace ZChain.Hashers;

public class MyHasher : IHasher
{
    public string ComputeHash(string input)
    {
        // Implementation - return hex string
    }
}
```

2. Add tests in `ZChain.Tests/UnitTests/Domain/HasherTests/`

3. Consider thread safety (use thread-static or new instance per call)

### Adding a New Miner

1. Create in `ZChain.CpuMiner` or new project:
```csharp
namespace ZChain.CpuMiner;

public class MyMiner<T>(IHasher hasher, int config) : IMiner<T>
    where T : class
{
    public async Task MineBlock(Block<T> block)
    {
        block.BeginMining();
        // Mining logic - find hash matching difficulty
        block.SetMinedValues(hash, nonce);
    }
}
```

2. Support CancellationToken for graceful shutdown
3. Add integration tests in `ZChain.Tests/Integration/`

### Extending Block<T>

Avoid modifying Block<T> directly. Instead:
- Create wrapper/decorator classes
- Use composition over inheritance
- Add extension methods for new behaviors

## Test Requirements

### Unit Tests (Required)

Location: `ZChain.Tests/UnitTests/Domain/{Feature}Tests/`

```csharp
public class MyFeatureTests
{
    [Fact]
    public void WhenCondition_AndContext_ShouldExpectedBehavior()
    {
        // Arrange
        // Act
        // Assert with Shouldly
    }
}
```

### Integration Tests (For workflows)

Location: `ZChain.Tests/Integration/`

```csharp
[Theory]
[InlineData(1, 1)]
[InlineData(2, 2)]
public async Task MyFeature_WithParameters_ShouldWork(int param1, int param2)
{
    // Full workflow test
}
```

## Checklist

- [ ] Code follows file-scoped namespace style
- [ ] Public APIs have null checks (ArgumentNullException)
- [ ] Async methods use CancellationToken where appropriate
- [ ] Unit tests cover happy path and edge cases
- [ ] Integration test for full workflow (if applicable)
- [ ] Build passes with no warnings (`dotnet build`)
- [ ] All tests pass (`dotnet test`)

## Commands

```bash
# Build and verify
dotnet build src/ZChain.sln

# Run tests
dotnet test src/ZChain.sln

# Run specific test class
dotnet test --filter "FullyQualifiedName~MyFeatureTests"
```
