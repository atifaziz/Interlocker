using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Interlocker;

namespace Collections.Pooled.Benchmarks.PooledList
{
    [CoreJob]
    [MemoryDiagnoser]
    public class InterlockerBenchmarks
    {

        [Benchmark]
        public void InterlockerOptimizedExceptTests()
        {
            var i = Interlocker.InterlockedOptimized.Create("a");
            for (var k = 0; k < N; k++)
            {
                i.UpdateExcept(x => x == "a" ? "b" : "a");
            }
        }


        [Benchmark]
        public void InterlockerOptimizedShortTests()
        {
            var i = Interlocker.InterlockedOptimized.Create("a");
            for (var k = 0; k < N; k++)
            {
                i.UpdateShort(x => x == "a" ? "b" : "a");
            }
        }

        [Benchmark]
        public void InterlockerOptimizedNoChecksTests()
        {
            var i = Interlocker.InterlockedOptimized.Create("a");
            for (var k = 0; k < N; k++)
            {
                i.Update(x => x == "a" ? "b" : "a");
            }
        }

        [Benchmark]
        public void InterlockerTests()
        {
            var i = Interlocker.Interlocked.Create("a");
            for (var k = 0; k < N; k++)
            {
                i.Update(x => x == "a" ? "b" : "a");
            }
        }



        [Params(100_000)]
        public int N;
    }
}
