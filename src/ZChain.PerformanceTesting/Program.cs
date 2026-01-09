using System;
using BenchmarkDotNet.Running;

namespace ZChain.PerformanceTesting;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<MiningSpeed>();
        Console.ReadLine();
    }
}
