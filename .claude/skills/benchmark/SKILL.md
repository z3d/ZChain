---
name: benchmark
description: Run BenchmarkDotNet performance tests and analyze results. Use when the user mentions benchmarks, performance testing, mining speed, or wants to compare performance.
allowed-tools: Read, Bash, Glob, Grep
---

# Performance Benchmarking

Run and analyze mining performance benchmarks using BenchmarkDotNet.

## Quick Start

Run full benchmark suite:
```bash
dotnet run -c Release --project src/ZChain.PerformanceTesting/ZChain.PerformanceTesting.csproj
```

## Benchmark Parameters

| Parameter | Values | Description |
|-----------|--------|-------------|
| ThreadCount | 1, 2, 3, 10 | Number of parallel mining threads |
| Difficulty | 1, 2, 3 | Leading zeros required in hash |

## Results Location

Results are saved to: `BenchmarkDotNet.Artifacts/results/`

Key files:
- `*-report-github.md` - Markdown table for GitHub/documentation
- `*-report.csv` - Raw data for analysis
- `*-report.html` - Interactive HTML report

## Interpreting Results

| Metric | Meaning | Healthy Range |
|--------|---------|---------------|
| Mean | Average execution time | Varies by difficulty |
| Error | Half of 99.9% CI | <10% of Mean |
| StdDev | Standard deviation | <25% of Mean |
| Median | Middle value | Close to Mean |

### Variance Guidelines

- **1-10% variance**: Excellent measurement stability
- **10-25% variance**: Acceptable, minor environmental noise
- **>25% variance**: Investigate - close background apps, check thermal throttling

## Comparing Results

To compare against baseline:
1. Current baseline in `BenchmarkDotNet.Artifacts/results/*-baseline.csv`
2. Run new benchmark
3. Compare Mean values - >10% regression warrants investigation

## Troubleshooting

### High Variance
- Close resource-intensive applications
- Disable CPU throttling/power saving
- Ensure system isn't thermally throttling
- Run multiple times to identify outliers

### BenchmarkDotNet Version
Using 0.14.0 (pinned). Version 0.15.x showed unstable measurements (5-34% variance vs 1-21% with 0.14.0).

## Workflow

1. **Before changes**: Run baseline benchmark
2. **Make changes**: Implement feature/optimization
3. **After changes**: Run comparison benchmark
4. **Analyze**: Compare Mean times, check variance
5. **Document**: Note significant changes in PR description
