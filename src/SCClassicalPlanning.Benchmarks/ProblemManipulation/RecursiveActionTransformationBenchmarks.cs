﻿using BenchmarkDotNet.Attributes;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;

namespace SCClassicalPlanning.Benchmarks.ProblemManipulation
{
    [MemoryDiagnoser]
    [InProcess]
    public class RecursiveActionTransformationBenchmarks
    {
        public record TestCase(string Label, bool DoSomething, Action Action)
        {
            public override string ToString() => Label;
        }

        public static IEnumerable<TestCase> TestCases { get; } = new TestCase[]
        {
            new(
                Label: "BlocksWorld Move NO-OP",
                DoSomething: false,
                Action: BlocksWorld.Move(new VariableDeclaration("b"), new VariableDeclaration("f"), new VariableDeclaration("t"))),

            new(
                Label: "BlocksWorld Move ALL-LEAFS-OP",
                DoSomething: true,
                Action: BlocksWorld.Move(new VariableDeclaration("b"), new VariableDeclaration("f"), new VariableDeclaration("t"))),
        };

        [ParamsSource(nameof(TestCases))]
        public TestCase? CurrentTestCase { get; set; }

        [Benchmark(Baseline = true)]
        public Action CurrentImpl() => new VarTransform(CurrentTestCase!.DoSomething).ApplyTo(CurrentTestCase!.Action);

        [Benchmark]
        public Action ToList() => new VarTransform_ToList(CurrentTestCase!.DoSomething).ApplyTo(CurrentTestCase!.Action);

        [Benchmark]
        public Action IterateTwice() => new VarTransform_IterateTwice(CurrentTestCase!.DoSomething).ApplyTo(CurrentTestCase!.Action);

        private class VarTransform_IterateTwice : RecursiveActionTransformation_IterateTwice
        {
            private readonly bool doSomething;

            public VarTransform_IterateTwice(bool doSomething) => this.doSomething = doSomething;

            public override VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
            {
                return doSomething ? new VariableDeclaration("X") : variableDeclaration;
            }
        }

        private class VarTransform_ToList : RecursiveActionTransformation_ToIHS
        {
            private readonly bool doSomething;

            public VarTransform_ToList(bool doSomething) => this.doSomething = doSomething;

            public override VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
            {
                return doSomething ? new VariableDeclaration("X") : variableDeclaration;
            }
        }

        private class VarTransform : RecursiveActionTransformation
        {
            private readonly bool doSomething;

            public VarTransform(bool doSomething) => this.doSomething = doSomething;

            public override VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
            {
                return doSomething ? new VariableDeclaration("X") : variableDeclaration;
            }
        }
    }
}
