using System;
using BenchmarkDotNet.Running;

namespace PerformanceTesting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<MiningSpeed>();
            Console.ReadLine();
        }
    }
}
