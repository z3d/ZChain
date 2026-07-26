# ZChain

A .NET blockchain implementation with Proof-of-Work mining, built for learning and performance experimentation.

## Commands

```bash
dotnet format src/ZChain.sln
dotnet build src/ZChain.sln            # warnings are errors
dotnet test src/ZChain.sln
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj
```

Format, build, and test before committing.

## Design

`ZChain.Core` is the domain layer and depends on nothing. `ZChain.Hashers` and `ZChain.CpuMiner` are pluggable implementations behind `IHasher` and `IMiner<T>`; a new hashing algorithm or a GPU miner is a new class in (or beside) those projects, not a change to Core.

`Block<T>` is a state machine — `New → Mining → Mined` via `BeginMining()` then `SetMinedValues(hash, nonce)`, with `Verify()` validating integrity on a mined block. **Any other transition throws `BlockStateException`**, which is the invariant the whole design rests on: a block can't be re-mined, mutated after mining, or verified before it exists. Preserve it when adding states or miners.

`BlockBuilder<T>` enforces required dependencies at compile time rather than at runtime — keep new required dependencies in the type-state chain rather than adding a runtime null check.

## Conventions

`.editorconfig` enforces the mechanical parts (180-char lines, file-scoped namespaces, analyzer diagnostics as errors). Beyond that:

- `var` only where the type is already obvious from the right-hand side — `var items = new List<string>()` yes, `var result = GetResult()` no.
- Primary constructors for simple DI; pattern matching over type checks; `ArgumentNullException.ThrowIfNull()` for guards.
- No `#region`, no commented-out code, no XML doc comments.

## Testing

xUnit + Shouldly in `src/ZChain.Tests/`. Names read `WhenCondition_AndContext_ShouldExpectedBehavior`, bodies are Arrange-Act-Assert.

**Stubs, not mocks** — `StubHasher`, `StubMiner<T>`. There is deliberately no mocking framework here: mining and hashing are deterministic, so a stub that computes a real (trivial) answer tests more than a mock that asserts a call happened.

## Benchmarking

BenchmarkDotNet is **pinned to 0.14.0** — 0.15.x showed high variance and made results incomparable. Don't bump it without re-establishing a baseline.

Runs sweep ThreadCount 1/2/3/10 against difficulty 1/2/3 (leading zeros). Results land in `BenchmarkDotNet.Artifacts/results/`, which is machine-specific and never committed. Variance above ~25% means the run is untrustworthy, not that the change was slow.

## Where to look

| Task | Skill |
|---|---|
| Adding a hasher, miner, or transaction type | `.claude/skills/new-feature/SKILL.md` |
| Running and interpreting benchmarks | `.claude/skills/benchmark/SKILL.md` |
| Branches, commits, PRs | `.claude/skills/pr-workflow/SKILL.md` |
| Auditing crypto or consensus code | `.claude/skills/security-review/SKILL.md` |
