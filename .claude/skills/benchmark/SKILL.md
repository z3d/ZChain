---
name: benchmark
description: Run BenchmarkDotNet performance tests and analyze results. Use when the user mentions benchmarks, performance testing, mining speed, or wants to compare performance.
allowed-tools: Read, Bash, Glob, Grep
---

# Performance Benchmarking

```bash
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj
```

Sweeps ThreadCount 1/2/3/10 against Difficulty 1/2/3 (leading zeros required in the hash). Output lands in `BenchmarkDotNet.Artifacts/results/` as a GitHub-flavoured markdown table, a CSV, and an HTML report — all machine-specific, none committed.

Always benchmark before *and* after a change on the same machine in the same session. A number without its own baseline says nothing. Read the Traps in `AGENTS.md` first: the NuGet audit needs `NuGetAudit=false` in the environment, and a single row of the sweep can be a lucky draw.

## Reading the result

**Check variance before you look at the mean.** StdDev above ~25% of Mean means the run is untrustworthy and the mean is noise — close background applications, check for thermal throttling or power saving, and re-run. Under 10% is a clean measurement; 10–25% is usable with care.

Error (half the 99.9% CI) should sit under 10% of Mean, and a Median far from Mean signals outliers rather than a shifted distribution.

Once variance is clean, a Mean regression beyond ~10% against `*-baseline.csv` is worth investigating; anything smaller is inside the noise floor of this suite.

## The version pin is load-bearing

BenchmarkDotNet is pinned to **0.14.0**. 0.15.x measured the same workload at 5–34% variance where 0.14.0 gave 1–21%, which is the difference between a usable signal and none. Don't bump it casually, and if you do, re-establish every baseline — results across versions aren't comparable.

## Related skills

- `new-feature` — implementing the miner or hasher being measured
- `pr-workflow` — noting significant performance changes in the PR
