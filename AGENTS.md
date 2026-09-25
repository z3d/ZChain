# ZChain

A .NET blockchain implementation with Proof-of-Work mining, built for learning and performance experimentation. About 500 lines of code; the value is in the mining loop and the measurements around it.

## Commands

```bash
dotnet format src/ZChain.sln
dotnet build src/ZChain.sln            # warnings are errors
dotnet test src/ZChain.sln
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj
```

Format, build, and test before committing. A `PreToolUse` hook in `.claude/settings.json` runs all three on every `git commit` and refuses the commit if any of them fails, so the checklist is enforced rather than asked for.

## Design

`ZChain.Core` is the domain layer and depends on nothing. `ZChain.Hashers` and `ZChain.CpuMiner` are pluggable implementations behind `IHasher` and `IMiner<T>`; a new hashing algorithm or a GPU miner is a new class in (or beside) those projects, not a change to Core.

`Block<T>` is a state machine — `New → Mining → Mined` via `SetMiningBeginning()` then `SetMinedValues(nonce, hash)`, with `VerifyMinedBlock()` walking the chain back to genesis and recomputing every hash. **Any other transition throws**, which is the invariant the whole design rests on: a block can't be re-mined, mutated after mining, or verified before it exists. Preserve it when adding states or miners.

`IHasher` is span-based — bytes in, digest written into a caller-owned span — because the miner calls it millions of times a second and a per-call allocation shows up as gen0 pauses across every mining thread. `Block<T>.SatisfiesDifficulty` checks leading zero nibbles on the raw digest; `CalculateHash(string)` is the verification path and the only place a hex string is built.

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

## Traps

Each of these cost real time, and none shows up in a diff.

- **NuGet audit fails on this machine.** The global NuGet config lists a private Azure feed that is unreachable, and `NU1900` is a warning-as-error, so every restore fails. Set `NuGetAudit=false` as an environment variable, not as a `-p:` switch — BenchmarkDotNet spawns its own build, which only inherits the environment.
- **A single benchmark row can be a lucky draw.** Nonces are deterministic, so BenchmarkDotNet's fixed block always finds the same winner and one parameter row may sit far from the expected work. Compare the whole sweep, or measure hash rate with a harness that chains many random blocks.
- **GitHub disables the CodeQL workflow after 60 days without activity**, and the code-scanning ruleset then blocks every PR, including docs-only ones. `gh workflow enable codeql.yml`, then close and reopen the PR to retrigger the scan.
- **Ten threads is not ten cores.** This box is 4 physical cores with hyperthreading; SHA-256 throughput caps around 4–5x however many threads you add. Read `ThreadCount=10` results against that ceiling.

## Recorded decisions

Each of these was chosen against a reasonable alternative and carries a **re-add trigger** — the specific fact that would justify revisiting it. Don't reverse one without hitting its trigger.

| Decision | Re-add trigger |
|---|---|
| BenchmarkDotNet pinned at 0.14.0 | A 0.15.x release measures this suite under ~25% variance; re-baseline everything when bumping |
| `IHasher` is span-based, not string-based | None — a string hasher can wrap the span one, and the reverse costs 2x hash rate |
| Nonces are per-thread interleaved counters, not Guids | A nonce ever needs to be unguessable; Proof-of-Work gets its security from the hash, not nonce secrecy |
| `Sha256Hasher` reuses a thread-static instance rather than `SHA256.HashData` | .NET's one-shot path stops opening a fresh CNG handle per call; measure on Windows before switching |
| Windows CNG SHA-256 is the floor; no managed implementation | Transactions grow past a few hundred bytes, where a midstate over the constant prefix would save more per guess than the CNG hardware path gives up |

## Where to look

| Task | Skill |
|---|---|
| Adding a hasher, miner, or transaction type | `.claude/skills/new-feature/SKILL.md` |
| Running and interpreting benchmarks | `.claude/skills/benchmark/SKILL.md` |
| Branches, commits, PRs | `.claude/skills/pr-workflow/SKILL.md` |
| Auditing crypto or consensus code | `.claude/skills/security-review/SKILL.md` |

This file is the one set of instructions for every agent harness. `CLAUDE.md` is a one-line `@AGENTS.md` import, kept for Claude sessions that cannot read `AGENTS.md` directly; the import never loads it twice. There is no mirror to keep in step; edit this file and `.claude/skills/` only.
