# ZChain

A .NET-based blockchain implementation with Proof-of-Work mining, built for learning and performance experimentation.

## Quick Reference

```bash
# Build
dotnet build src/ZChain.sln

# Test
dotnet test src/ZChain.sln

# Run benchmarks (from solution root)
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj

# Run console demo
dotnet run --project src/ZChain.ConsoleApp/ZChain.ConsoleApp.csproj
```

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

## Code Conventions

- **Framework**: .NET 10.0 (LTS)
- **Style**: File-scoped namespaces, primary constructors where appropriate
- **Nullability**: Enabled in newer projects (ZChain.Hashers)
- **Warnings**: Treated as errors in both Debug and Release
- **Assertions**: Shouldly for BDD-style test assertions

### Naming

- PascalCase: Classes, methods, properties, public members
- camelCase: Private fields, local variables, parameters
- Prefix interfaces with `I` (e.g., `IHasher`, `IMiner<T>`)

## Testing

- **Unit tests**: `src/ZChain.Tests/UnitTests/` - isolated component tests with stubs
- **Integration tests**: `src/ZChain.Tests/Integration/` - full mining workflows
- **Stubs**: `StubMiner<T>`, `StubHasher` for deterministic test behavior

Run specific test:
```bash
dotnet test --filter "FullyQualifiedName~BlockTests"
```

## Performance Benchmarking

Uses BenchmarkDotNet 0.14.0 (pinned for measurement stability - 0.15.x showed 5-34% variance).

Benchmark parameters:
- **ThreadCount**: 1, 2, 3, 10
- **Difficulty**: 1, 2, 3 (leading zeros in hash)

Results saved to: `BenchmarkDotNet.Artifacts/results/`

Expected variance with 0.14.0: 1-21% across runs. Higher variance indicates environmental issues.

## Dependencies

| Package | Purpose |
|---------|---------|
| Newtonsoft.Json | Block serialization/deserialization |
| BenchmarkDotNet | Performance measurement |
| xunit + Shouldly | Testing framework |

## Git Workflow

- Branch naming: `feature/`, `fix/`, `chore/` prefixes
- PRs target `main` branch
- CodeRabbit reviews enabled
- GitHub Actions: build + test on PR, CodeQL security scanning

## Import References

- @src/ZChain.Core/Block.cs - Core block implementation
- @src/ZChain.CpuMiner/CpuMiner.cs - Mining logic
- @.github/workflows/dotnet.yml - CI pipeline
