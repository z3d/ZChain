# Code Style Guidelines

## C# Conventions

### Modern C# Features (Prefer)

- File-scoped namespaces: `namespace ZChain.Core;`
- Primary constructors for simple DI: `public class Foo(IDep dep)`
- Target-typed new: `Block<T> block = new();`
- Pattern matching in conditionals
- Expression-bodied members for single-line implementations

### Avoid

- Traditional namespace blocks with braces (legacy style)
- `var` when type isn't obvious from context
- Regions (`#region`) - use smaller classes instead
- Commented-out code - delete it, git has history

## Project Structure

### New Classes

Place in appropriate project:
- Domain models/interfaces → `ZChain.Core`
- Mining implementations → `ZChain.CpuMiner`
- Hashing algorithms → `ZChain.Hashers`
- Tests mirror source structure in `ZChain.Tests`

### Builders

Use fluent builder pattern for complex object construction:
```csharp
var block = new BlockBuilder<Transaction>()
    .WithPreviousBlock(parent)
    .WithTransaction(tx)
    .WithDifficulty(3)
    .WithHasher(hasher)
    .Build();
```

## Error Handling

- Throw `ArgumentNullException` for null parameters in public APIs
- Throw `ArgumentOutOfRangeException` for invalid numeric ranges
- Use domain-specific exceptions (e.g., `BlockStateException`) for business rule violations
- Never catch and swallow exceptions silently

## Async/Concurrency

- Suffix async methods with `Async` only when sync overload exists
- Always pass `CancellationToken` through async call chains
- Use `Task.WhenAny` for racing operations, not `Task.WaitAny`
- Dispose `CancellationTokenSource` properly

## Documentation

- XML docs only for public API surface intended for external consumption
- Prefer self-documenting code over comments
- Comments explain "why", not "what"
- Keep README.md minimal - this is a learning project
