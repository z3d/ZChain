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

`IHasher` is span-based — bytes in, digest written into a caller-owned span — because the miner calls it millions of times a second and a per-call allocation shows up as gen0 pauses across every mining thread. Its `ComputeHashes` default loops `ComputeHash`; `Sha256Hasher` overrides it to run 8 equal-length inputs as AVX2 lanes in pure C# (`Sha256Vector8`), and when the 8 inputs share their leading whole 64-byte blocks it caches that prefix's midstate per thread so each input only compresses its tail. The hash input is therefore **block bytes then nonce**, never the other way round. `Block<T>.FindNonceSatisfyingDifficulty` hashes a batch of nonces and checks leading zero nibbles on the raw digests; `CalculateHash(string)` is the verification path and the only place a hex string is built.

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
- **This laptop throttles.** An i7-10510U under sustained AVX2 load swings between 0.8 and 4.9 GHz, so absolute numbers move 2–3x between runs minutes apart. Only a before/after interleaved in the same run is comparable; never compare against a number from an earlier session.

## Recorded decisions

Each of these was chosen against a reasonable alternative and carries a **re-add trigger** — the specific fact that would justify revisiting it. Don't reverse one without hitting its trigger.

| Decision | Re-add trigger |
|---|---|
| BenchmarkDotNet pinned at 0.14.0 | A 0.15.x release measures this suite under ~25% variance; re-baseline everything when bumping |
| `IHasher` is span-based, not string-based | None — a string hasher can wrap the span one, and the reverse costs 2x hash rate |
| Nonces are per-thread interleaved counters, not Guids | A nonce ever needs to be unguessable; Proof-of-Work gets its security from the hash, not nonce secrecy |
| `Sha256Hasher` reuses a thread-static instance rather than `SHA256.HashData` | .NET's one-shot path stops opening a fresh CNG handle per call; measure on Windows before switching |
| Mining hashes through a managed 8-lane AVX2 SHA-256 with a cached midstate, not Windows CNG | CNG's per-call overhead (~350 ns of a ~450 ns call) drops below the cost of one managed compression, or .NET exposes SHA-NI intrinsics — then a hardware path per lane wins again |
| Nonce goes after the block bytes in the hash input | None — nonce-first would make every 64-byte block differ per guess and throw the midstate away, which is 2x per guess |
| Verification still goes through CNG (`Sha256Hasher.ComputeHash`) | It doesn't need speed, and hashing the winner through an independent implementation is what catches a kernel bug — keep it that way |

## Where to look

| Task | Skill |
|---|---|
| Adding a hasher, miner, or transaction type | `.claude/skills/new-feature/SKILL.md` |
| Running and interpreting benchmarks | `.claude/skills/benchmark/SKILL.md` |
| Branches, commits, PRs | `.claude/skills/pr-workflow/SKILL.md` |
| Auditing crypto or consensus code | `.claude/skills/security-review/SKILL.md` |

This file is the one set of instructions for every agent harness. `CLAUDE.md` is a one-line `@AGENTS.md` import, kept for Claude sessions that cannot read `AGENTS.md` directly; the import never loads it twice. There is no mirror to keep in step; edit this file and `.claude/skills/` only.
