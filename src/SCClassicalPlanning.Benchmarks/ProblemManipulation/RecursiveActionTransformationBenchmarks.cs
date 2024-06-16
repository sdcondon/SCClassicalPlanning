using BenchmarkDotNet.Attributes;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;

namespace SCClassicalPlanning.Benchmarks.ProblemManipulation;

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
            Action: BlocksWorldDomain.Move(new VariableDeclaration("b"), new VariableDeclaration("f"), new VariableDeclaration("t"))),

        new(
            Label: "BlocksWorld Move ALL-LEAFS-OP",
            DoSomething: true,
            Action: BlocksWorldDomain.Move(new VariableDeclaration("b"), new VariableDeclaration("f"), new VariableDeclaration("t"))),
    };

    [ParamsSource(nameof(TestCases))]
    public TestCase? CurrentTestCase { get; set; }

    [Benchmark(Baseline = true)]
    public Action CurrentImpl() => new VarTransform(CurrentTestCase!.DoSomething).ApplyTo(CurrentTestCase!.Action);

    [Benchmark]
    public Action ToIHS() => new VarTransform_ToIHS(CurrentTestCase!.DoSomething).ApplyTo(CurrentTestCase!.Action);

    [Benchmark]
    public Action Linq() => new VarTransform_Linq(CurrentTestCase!.DoSomething).ApplyTo(CurrentTestCase!.Action);

    private class VarTransform_Linq : RecursiveActionTransformation_Linq
    {
        private readonly bool doSomething;

        public VarTransform_Linq(bool doSomething) => this.doSomething = doSomething;

        public override VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
        {
            return doSomething ? new VariableDeclaration("X") : variableDeclaration;
        }
    }

    private class VarTransform_ToIHS : RecursiveActionTransformation_ToIHS
    {
        private readonly bool doSomething;

        public VarTransform_ToIHS(bool doSomething) => this.doSomething = doSomething;

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
