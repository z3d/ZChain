---
name: security-review
description: Security review for blockchain and cryptographic code. Use when reviewing PRs, auditing code, checking for vulnerabilities, or when security is mentioned.
allowed-tools: Read, Grep, Glob
---

# Security Review

What actually goes wrong in this codebase, in rough order of severity.

## Hash input completeness

The hash must cover **every** field that defines the block — height, parent hash, transaction, nonce. A hash over the nonce alone (or over any proper subset) means two different blocks can share a hash, which collapses chain integrity. This is the highest-value thing to check on any change to hashing or block structure.

SHA256 or stronger, never MD5/SHA1. Nonces must not be predictable, and anything security-sensitive uses `RandomNumberGenerator`, never `System.Random`.

## State machine and concurrency

`Block<T>`'s state machine is the integrity guarantee, and mining is multi-threaded, so the two interact. Check that a mined block can't be re-mined or mutated, an unmined block can't be verified, and — the subtle one — that the transition to `Mined` is **atomic under contention**. Many threads race to find a nonce; without a lock (or an interlocked equivalent), two winners can both write hash and nonce.

Also check that `CancellationToken` is propagated through long-running mining loops and that `CancellationTokenSource` instances are disposed. A leaked-but-cancelled miner burns CPU silently.

## Deserialization

`TypeNameHandling` on Newtonsoft.Json permits arbitrary type instantiation from untrusted input — treat any occurrence as Critical. Deserialize to an explicit closed generic (`Block<MoneyTransferTransaction>`), and validate what comes back rather than trusting it.

## Boundaries and exposure

Null checks and range validation on public parameters (`ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(difficulty)`). No key material or sensitive state in logs, exception messages, debug output, or benchmark artifacts. JSON serialization shouldn't expose internal fields.

Dependencies come from trusted sources, pinned to specific versions, with Dependabot alerts triaged rather than ignored.

## Fast scan

```bash
grep -rn "Random()" src/
grep -rn "MD5\|SHA1" src/
grep -rn "TypeNameHandling" src/
grep -rn "Process.Start\|Shell" src/
grep -rn "password\|secret\|key\|token" src/ --include="*.cs"
```

Grep finds candidates, not findings — confirm each by reading the surrounding code before reporting it.

## Severity

**Critical** — exploitable now (arbitrary type instantiation, an incomplete hash input). Block the merge.
**High** — a real weakness needing a specific precondition (a racy state transition). Fix before merge.
**Medium** — defense in depth. Track it.
**Low** — best-practice deviation. Note it.

## Related skills

- `new-feature` — the constraints new hashers and miners must satisfy
- `pr-workflow` — where review sits in the merge process
