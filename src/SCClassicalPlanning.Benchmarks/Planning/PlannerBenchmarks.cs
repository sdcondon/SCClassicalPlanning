using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCClassicalPlanning.Benchmarks.Planning
{
    public class PlannerBenchmarks
    {
    }

    [MemoryDiagnoser]
    [InProcess]
    public class PlannerBenchmarks
    {
        [Benchmark]
        public static bool CrimeExample_SimpleBackwardChainingKnowledgeBase()
        {
            var kb = new SimpleBackwardChainingKnowledgeBase();
            kb.TellAsync(CrimeDomain.Axioms).Wait();
            return kb.AskAsync(IsCriminal(West)).GetAwaiter().GetResult();
        }

        [Benchmark]
        public static bool CrimeExample_BackwardChainingKnowledgeBase_FromAIaMA()
        {
            var kb = new BackwardChainingKnowledgeBase_FromAIaMA();
            kb.TellAsync(CrimeDomain.Axioms).Wait();
            return kb.AskAsync(IsCriminal(West)).GetAwaiter().GetResult();
        }
    }
}
