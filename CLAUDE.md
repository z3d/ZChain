# ZChain

A .NET-based blockchain implementation with Proof-of-Work mining, built for learning and performance experimentation.

## Quick Reference

```bash
# Format code
dotnet format src/ZChain.sln

# Build
dotnet build src/ZChain.sln

# Test
dotnet test src/ZChain.sln

# Run benchmarks (Release mode required)
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj

# Run console demo
dotnet run --project src/ZChain.ConsoleApp/ZChain.ConsoleApp.csproj
```

## Pre-Commit Checklist

Before every commit:

1. `dotnet format src/ZChain.sln` - Apply formatting standards
2. `dotnet build src/ZChain.sln` - Verify compilation (warnings are errors)
3. `dotnet test src/ZChain.sln` - All tests must pass

## Architecture

```
src/
├── ZChain.Core/           # Domain models, interfaces, builders
├── ZChain.CpuMiner/       # Multi-threaded CPU mining implementation
├── ZChain.Hashers/        # SHA256 hashing (IHasher implementations)
├── ZChain.Tests/          # xUnit tests (unit + integration)
├── ZChain.ConsoleApp/     # Demo application
└── ZChain.PerformanceTesting/  # BenchmarkDotNet suite
```

### Core Components

- **Block<T>**: Generic block with state machine (New → Mining → Mined)
- **CpuMiner<T>**: Multi-threaded PoW miner with cancellation support
- **BlockBuilder<T>**: Fluent API for block construction
- **IHasher/IMiner<T>**: Abstractions for hashing and mining strategies

### Key Design Decisions

- Generic transaction type `T` allows any payload in blocks
- Thread-first-wins mining race with proper cancellation
- Builder pattern enforces required dependencies at compile time
- State machine prevents invalid block operations

## Code Style

Enforced via `.editorconfig`:

- **Line length**: 180 characters max
- **Indentation**: 4 spaces
- **Namespaces**: File-scoped required (`namespace Foo;`)
- **Usings**: Outside namespace, System first
- **Security**: Analyzer diagnostics treated as errors

### Prohibited Patterns

- ❌ Code regions (`#region`) - use smaller classes instead
- ❌ Historical/commented-out code - use git history
- ❌ XML documentation comments in application code
- ❌ `var` when type isn't apparent from context

### Required Patterns

- ✅ File-scoped namespaces
- ✅ Primary constructors for simple DI
- ✅ Expression-bodied members for single-line implementations
- ✅ Pattern matching over type checks
- ✅ Nullable reference types in new code

## Testing

- **Framework**: xUnit with Shouldly assertions
- **Unit tests**: `src/ZChain.Tests/UnitTests/` - isolated with stubs
- **Integration tests**: `src/ZChain.Tests/Integration/` - full workflows
- **Stubs**: `StubMiner<T>`, `StubHasher` for deterministic behavior

### Test Naming Convention

```csharp
[Fact]
public void WhenCondition_AndContext_ShouldExpectedBehavior()
```

### Running Tests

```bash
# All tests
dotnet test src/ZChain.sln

# Specific test class
dotnet test --filter "FullyQualifiedName~BlockTests"

# With detailed output
dotnet test --verbosity normal
```

## Performance Benchmarking

Uses BenchmarkDotNet 0.14.0 (pinned - 0.15.x showed 5-34% variance vs 1-21%).

Parameters:
- **ThreadCount**: 1, 2, 3, 10
- **Difficulty**: 1, 2, 3 (leading zeros in hash)

Results: `BenchmarkDotNet.Artifacts/results/`

Healthy variance: <25%. Higher indicates environmental issues.

## Dependencies

| Package | Purpose |
|---------|---------|
| Newtonsoft.Json | Block serialization/deserialization |
| BenchmarkDotNet 0.14.0 | Performance measurement (pinned) |
| xunit 2.9.3 | Unit testing |
| Shouldly | BDD-style assertions |

## Git Workflow

- Branch naming: `feature/`, `fix/`, `chore/` prefixes
- PRs target `main` branch
- Squash merge to keep history clean
- CodeRabbit reviews enabled
- GitHub Actions: build + test on PR, CodeQL security scanning

## Import References

- @src/ZChain.Core/Block.cs - Core block implementation
- @src/ZChain.CpuMiner/CpuMiner.cs - Mining logic
- @.github/workflows/dotnet.yml - CI pipeline
- @.editorconfig - Code style rules
