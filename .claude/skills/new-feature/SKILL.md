---
name: new-feature
description: Guide for implementing new features in ZChain. Use when adding new functionality, creating new classes, extending the blockchain, or implementing new miners/hashers.
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
---

# Implementing New Features

Most extensions are a new implementation of an existing interface, not a change to `ZChain.Core`. Read the nearest existing implementation before writing a new one.

| Adding | Goes in | Implements |
|---|---|---|
| Transaction type | the consuming project, or Core | nothing — it's the `T` in `Block<T>` |
| Hash algorithm | `ZChain.Hashers` | `IHasher` |
| Mining strategy | `ZChain.CpuMiner`, or a new project | `IMiner<T>` |

## Constraints that aren't obvious

**Hashers must be safe under concurrent mining.** `CpuMiner<T>` runs many threads against one hasher, so a hasher holding mutable state (a reused `HashAlgorithm` instance, a buffer field) will corrupt results non-deterministically rather than fail. Use a thread-static or a fresh instance per call.

**Miners must honour cancellation.** `CpuMiner<T>` is cancellable so a losing thread stops the moment another finds the nonce. A miner that ignores its `CancellationToken` leaves threads burning after the block is mined.

**Miners drive the state machine, they don't bypass it.** `block.BeginMining()`, find the nonce, `block.SetMinedValues(hash, nonce)`. Any other sequence throws `BlockStateException` — that is the design working, not an obstacle.

**Don't modify `Block<T>` to add behaviour.** Compose: a wrapper, a decorator, or an extension method. The state machine is the one thing every other component trusts.

**Don't add a required dependency as a runtime null check.** `BlockBuilder<T>`'s type-state chain makes missing dependencies a compile error; keep new requirements there.

## Tests

Unit tests in `ZChain.Tests/UnitTests/Domain/{Feature}Tests/`, integration tests for a full mining workflow in `ZChain.Tests/Integration/` (`[Theory]` over difficulty and thread count is the established shape). xUnit + Shouldly, `WhenCondition_AndContext_ShouldExpectedBehavior`, Arrange-Act-Assert, stubs rather than mocks.

Cover the invalid-transition paths, not just the happy path — a new miner that can reach `SetMinedValues` twice is exactly the bug the state machine exists to catch.

```bash
dotnet build src/ZChain.sln
dotnet test src/ZChain.sln
dotnet test --filter "FullyQualifiedName~MyFeatureTests"
```

## Related skills

- `security-review` — what to check before merging crypto or concurrency changes
- `benchmark` — measuring a new miner or hasher against the pinned baseline
