---
name: security-review
description: Security review for blockchain and cryptographic code. Use when reviewing PRs, auditing code, checking for vulnerabilities, or when security is mentioned.
allowed-tools: Read, Grep, Glob
---

# Security Review

Checklist for reviewing ZChain code for security issues.

## Cryptographic Security

### Hash Function Usage

- [ ] Using SHA256 (or stronger) - never MD5/SHA1
- [ ] Hash inputs include all relevant block data
- [ ] No predictable nonce generation
- [ ] Proper hex encoding of hash output

```csharp
// GOOD: All block data in hash
var hashInput = $"{block.Height}{block.ParentHash}{block.Transaction}{nonce}";

// BAD: Missing fields allows hash collision attacks
var hashInput = $"{nonce}";
```

### Random Number Generation

- [ ] Using cryptographically secure RNG for security-sensitive operations
- [ ] Not using `System.Random` for cryptographic purposes

```csharp
// GOOD: Cryptographically secure
using var rng = RandomNumberGenerator.Create();
byte[] bytes = new byte[32];
rng.GetBytes(bytes);

// BAD: Predictable
var random = new Random();
```

## Blockchain Integrity

### Block Validation

- [ ] Verify hash matches recorded values
- [ ] Verify parent hash chain is intact
- [ ] Verify difficulty requirement is met
- [ ] Verify block state transitions are valid

### State Machine Security

- [ ] Cannot mine already-mined block
- [ ] Cannot verify unmined block
- [ ] Cannot modify mined block values
- [ ] State transitions are thread-safe

## Concurrency Issues

### Thread Safety

- [ ] Shared state protected by locks or concurrent collections
- [ ] No race conditions in mining completion
- [ ] CancellationToken properly propagated
- [ ] Resources properly disposed

```csharp
// GOOD: Thread-safe mined value setting
lock (_minedLock)
{
    if (State == BlockState.Mined) return;
    _hash = hash;
    _nonce = nonce;
    State = BlockState.Mined;
}
```

### Cancellation

- [ ] Long-running operations check CancellationToken
- [ ] CancellationTokenSource disposed after use
- [ ] Graceful shutdown on cancellation

## Input Validation

### Public API Boundaries

- [ ] Null checks on all public method parameters
- [ ] Range validation on numeric inputs (difficulty > 0)
- [ ] No SQL injection (if database added)
- [ ] No command injection in Bash operations

```csharp
// GOOD: Proper validation
public Block(T transaction, int difficulty)
{
    ArgumentNullException.ThrowIfNull(transaction);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(difficulty);
}
```

## Sensitive Data

### Logging and Output

- [ ] No private keys in logs
- [ ] No sensitive data in exception messages
- [ ] Debug output doesn't expose internal state
- [ ] Benchmark results don't contain sensitive info

### Serialization

- [ ] JSON serialization doesn't expose internal fields
- [ ] Deserialization validates input
- [ ] No arbitrary type instantiation (TypeNameHandling)

```csharp
// BAD: Allows arbitrary type instantiation
JsonConvert.DeserializeObject<Block>(json, new JsonSerializerSettings {
    TypeNameHandling = TypeNameHandling.All  // DANGEROUS
});

// GOOD: Explicit type, no type handling
JsonConvert.DeserializeObject<Block<MoneyTransferTransaction>>(json);
```

## Dependency Security

- [ ] NuGet packages from trusted sources
- [ ] No known vulnerabilities (check GitHub Dependabot)
- [ ] Packages pinned to specific versions
- [ ] Regular dependency updates

## Review Commands

```bash
# Search for potential issues
grep -r "Random()" src/
grep -r "MD5\|SHA1" src/
grep -r "TypeNameHandling" src/
grep -r "Process.Start\|Shell" src/

# Check for hardcoded secrets
grep -r "password\|secret\|key\|token" src/ --include="*.cs"
```

## Severity Levels

| Level | Description | Action |
|-------|-------------|--------|
| Critical | Exploitable vulnerability | Block merge, fix immediately |
| High | Security weakness | Should fix before merge |
| Medium | Defense in depth issue | Track for future fix |
| Low | Best practice deviation | Note in review |
