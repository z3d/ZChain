# Testing Standards

## Test Organization

```
ZChain.Tests/
├── UnitTests/
│   └── Domain/
│       └── {Component}Tests/
│           └── {Component}Tests.cs
└── Integration/
    └── {Feature}Test.cs
```

## Unit Tests

### Naming Convention

```csharp
[Fact]
public void WhenCondition_AndContext_ShouldExpectedBehavior()
```

Example:
```csharp
[Fact]
public void WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow()
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
    block.RecordedTransaction.ShouldBe(transaction);
}
```

### Stubs Over Mocks

Use simple stub classes in `ZChain.Tests/`:
- `StubHasher` - returns predictable hash values
- `StubMiner<T>` - returns predetermined mining results

Avoid mocking frameworks - stubs are clearer and sufficient for this codebase.

## Integration Tests

### Use Theory for Parameterized Tests

```csharp
[Theory]
[InlineData(1, 1)]
[InlineData(2, 3)]
[InlineData(4, 2)]
public async Task Mining_WithVaryingParameters_ShouldSucceed(int threads, int difficulty)
{
    // Full workflow test
}
```

### Integration Test Scope

- Test complete mining workflows (multiple blocks)
- Verify blockchain integrity (parent references, heights)
- Validate state transitions through full lifecycle
- Use real implementations, not stubs

## Assertions with Shouldly

Prefer Shouldly for readable assertions:

```csharp
// Good
block.State.ShouldBe(BlockState.Mined);
block.Hash.ShouldStartWith("0000");
blocks.Count.ShouldBe(3);

// Avoid xUnit Assert
Assert.Equal(BlockState.Mined, block.State);  // Less readable
```

### Exception Testing

```csharp
Should.Throw<BlockStateException>(() => block.Mine());
await Should.ThrowAsync<InvalidOperationException>(async () => await miner.MineAsync(block));
```

## Performance Testing

### BenchmarkDotNet Benchmarks

Location: `ZChain.PerformanceTesting/`

```csharp
[Params(1, 2, 3, 10)]
public int ThreadCount { get; set; }

[Params(1, 2, 3)]
public int Difficulty { get; set; }

[Benchmark]
public async Task Mine()
{
    // Benchmark code
}
```

### Running Benchmarks

Always run in Release mode:
```bash
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj
```

### Interpreting Results

- Mean: Average execution time
- Error: Half of 99.9% confidence interval
- StdDev: Standard deviation
- Variance >25% across runs indicates measurement instability

## Test Commands

```bash
# All tests
dotnet test src/ZChain.sln

# Specific test class
dotnet test --filter "FullyQualifiedName~BlockTests"

# Specific test method
dotnet test --filter "WhenHavingAMinedBlock_AndAttemptingToRemine_ShouldThrow"

# With detailed output
dotnet test --verbosity normal

# With coverage (if coverlet installed)
dotnet test --collect:"XPlat Code Coverage"
```
