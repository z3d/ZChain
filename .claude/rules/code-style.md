# Code Style Guidelines

Enforced via `.editorconfig` - run `dotnet format src/ZChain.sln` before commits.

## Formatting Standards

- **Line length**: 180 characters maximum
- **Indentation**: 4 spaces (2 for JSON/YAML/XML)
- **Final newline**: Required in all files
- **Trailing whitespace**: Trimmed automatically

## C# Conventions

### Required (Warnings)

```csharp
// File-scoped namespaces - REQUIRED
namespace ZChain.Core;

// Usings outside namespace, System first
using System;
using System.Threading;
using Newtonsoft.Json;
```

### Modern C# Features (Prefer)

```csharp
// Primary constructors for simple DI
public class CpuMiner<T>(IHasher hasher, int threadCount) : IMiner<T>

// Expression-bodied members for single lines
public string Hash => _hash;
public bool IsValid() => Hash.StartsWith(new string('0', Difficulty));

// Pattern matching
if (state is BlockState.Mined)
return obj switch { null => "none", string s => s, _ => obj.ToString() };

// Target-typed new
Block<Transaction> block = new();
List<string> items = [];

// Null coalescing and propagation
var name = user?.Profile?.Name ?? "Unknown";
```

### Prohibited Patterns

| Pattern | Reason | Alternative |
|---------|--------|-------------|
| `#region` blocks | Hides code complexity | Smaller, focused classes |
| Commented-out code | Clutters codebase | Git history |
| `// TODO` without issue | Never gets done | Create GitHub issue |
| XML doc comments | Not a public library | Self-documenting code |
| `var` with unclear type | Reduces readability | Explicit type |
| Historical change comments | Noise | Git blame |

### var Usage

```csharp
// GOOD: Type is obvious
var items = new List<string>();
var block = new Block<Transaction>();
var dict = new Dictionary<string, int>();

// BAD: Type is not obvious - use explicit type
string result = GetResult();           // Not: var result = GetResult();
IHasher hasher = CreateHasher();       // Not: var hasher = CreateHasher();
Block<T> block = builder.Build();      // Not: var block = builder.Build();
```

## Project Organization

### File Placement

| Type | Location |
|------|----------|
| Domain models/interfaces | `ZChain.Core` |
| Mining implementations | `ZChain.CpuMiner` |
| Hashing algorithms | `ZChain.Hashers` |
| Unit tests | `ZChain.Tests/UnitTests/Domain/{Component}Tests/` |
| Integration tests | `ZChain.Tests/Integration/` |

### Class Structure Order

1. Constants and static fields
2. Instance fields
3. Constructors
4. Properties
5. Public methods
6. Private methods

## Error Handling

### Validation

```csharp
// Use ThrowIf methods (modern .NET)
ArgumentNullException.ThrowIfNull(transaction);
ArgumentOutOfRangeException.ThrowIfNegativeOrZero(difficulty);
ArgumentException.ThrowIfNullOrWhiteSpace(hash);

// Domain exceptions for business rules
if (State != BlockState.New)
    throw new BlockStateException($"Cannot mine block in {State} state");
```

### Exception Guidelines

- Throw `ArgumentNullException` for null public parameters
- Throw `ArgumentOutOfRangeException` for invalid ranges
- Throw domain exceptions (`BlockStateException`) for business rules
- Never catch and swallow exceptions silently
- Include meaningful messages with context

## Async/Concurrency

```csharp
// Pass CancellationToken through call chains
public async Task MineBlock(Block<T> block, CancellationToken ct = default)
{
    await DoWork(ct);
}

// Use Task.WhenAny for racing, not WaitAny
Task winner = await Task.WhenAny(tasks);

// Dispose CancellationTokenSource
using var cts = new CancellationTokenSource();

// Thread-safe state updates
lock (_lock)
{
    if (State == BlockState.Mined) return;
    _hash = hash;
    State = BlockState.Mined;
}
```

## Security Analyzers

Security diagnostics are treated as **errors** in `.editorconfig`:

```
dotnet_analyzer_diagnostic.category-Security.severity = error
```

This means security issues will fail the build. Do not suppress these without review.
