﻿// Copyright 2022-2024 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using Antlr4.Runtime;
using SCClassicalPlanning.ProblemCreation.Antlr;
using SCFirstOrderLogic;
using System.Diagnostics.CodeAnalysis;

namespace SCClassicalPlanning.ProblemCreation;

/// <summary>
/// <para>
/// PDDL parsing logic.
/// </para>
/// <para>
/// The language parsed by this class is absolutely minimal PDDL (1.2), based on the spec retrieved (on 12th March 2023) from:
/// https://homepages.inf.ed.ac.uk/mfourman/tools/propplan/pddl.pdf.
/// Absolutely NO extensions (not even typing) are recognised - STRIPS only.
/// Have also omitted problem situations and length specs. While they aren't marked as a particular
/// extension by the spec, they are marked as deprecated by this (well, length spec isn't even mentioned):
/// https://planning.wiki/ref/pddl/problem. That may just be due to incompleteness of this wiki, but
/// (for the moment at least) I'm taking every excuse I can to minimise this.
/// </para>
/// </summary>
// NB: not a fan of this class. It's all a bit ugly. Might experiment with some alternative approaches at some point (listener?)..
public static class PddlParser
{
    /// <summary>
    /// Parses a PDDL domain definition into a <see cref="PddlDomain"/> instance.
    /// </summary>
    /// <param name="pddl">The PDDL domain definition.</param>
    /// <returns>A new <see cref="PddlDomain"/> instance.</returns>
    public static PddlDomain ParseDomain(string pddl)
    {
        return TransformDomain(MakeParser(pddl).singleDomain().domain());
    }

    //// Perhaps at some point (for supporting :extends):
    //// public static PddlDomain ParseDomain(string pddl, Func<string, PddlDomain> getDomain)

    /// <summary>
    /// Parses a PDDL problem and domain definition pair into a new <see cref="Problem"/> instance.
    /// </summary>
    /// <param name="problemPddl">The PDDL problem definition.</param>
    /// <param name="domainPddl">The PDDL domain definition.</param>
    /// <returns>A new <see cref="Problem"/> instance.</returns>
    public static Problem ParseProblem(string problemPddl, string domainPddl)
    {
        return ParseProblem(problemPddl, ParseDomain(domainPddl));
    }

    /// <summary>
    /// Parses a PDDL problem definition into a new <see cref="Problem"/> instance.
    /// </summary>
    /// <param name="problemPddl">The PDDL problem definition.</param>
    /// <param name="domain">The domain that the problem resides in. NB with this method, no validation can occur that the problem matches the domain.</param>
    /// <returns>A new <see cref="Problem"/> instance.</returns>
    public static Problem ParseProblem(string problemPddl, PddlDomain domain)
    {
        // NB: doesn't verify problem's :domain element against domain's name, but probably should.
        return TransformProblem(MakeParser(problemPddl).singleProblem().problem(), s => domain);
    }

    /// <summary>
    /// Parses a PDDL problem definition into a new <see cref="Problem"/> instance.
    /// </summary>
    /// <param name="problemPddl">The PDDL problem definition.</param>
    /// <param name="getDomain">A delegate to look up the problem's domain. Will be passed the name given in the <c>:domain</c> element.</param>
    /// <returns>A new <see cref="Problem"/> instance.</returns>
    public static Problem ParseProblem(string problemPddl, Func<string, PddlDomain> getDomain)
    {
        return TransformProblem(MakeParser(problemPddl).singleProblem().problem(), getDomain);
    }

    private static MinimalPDDLParser MakeParser(string input)
    {
        AntlrInputStream inputStream = new(input);

        // NB: ANTLR apparently adds a listener by default that writes to the console.
        // Which is crazy default behaviour if you ask me, but never mind.
        // Remove it so that consumers of this lib don't get random messages turning up on their console.
        MinimalPDDLLexer lexer = new(inputStream);
        lexer.RemoveErrorListeners();
        CommonTokenStream tokens = new(lexer);

        // NB: In the parser, we add our own error listener that throws an exception.
        // Otherwise errors would just be ignored and the method would just return null, which is obviously bad behaviour.
        MinimalPDDLParser parser = new(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowingErrorListener.Instance);

        return parser;
    }

    private static PddlDomain TransformDomain(MinimalPDDLParser.DomainContext context)
    {
        var name = context.NAME().Symbol.Text;

        if (context.extendsDef() != null)
        {
            throw new NotSupportedException(":extends is not yet supported");
        }

        var requirementsFlags = context.requirementsDef()?._flags.Select(f => f.Text.ToLower());
        if (requirementsFlags != null && requirementsFlags.Any(f => !f.Equals(":strips")))
        {
            throw new NotSupportedException("Unsupported requirements flag(s) detected - only STRIPS is supported");
        }

        // NB: ignore :constants for now - easy enough to add later via new Domain ctor
        // Not that useful without typing, anyway (without typing, constants can just be implicit based on actions).

        // NB: ignore :predicates for now - without typing, its not useful anyway (existing predicates implicit based on actions), so all we'd be do is validating.
        // Might need richer model to support this (predicates with typed params essentially imply extra preconds on all actions that feature them in their preconds).
        // A few ways to go about that, but.. perhaps something like PredicateSchema(Predicate prototype, Sentence[] constraints)?
        // Something to consider AFTER v1.0 (and would be a breaking change to the model, so.. v2.0 or beyond).
        // ALTERNATIVELY, could perhaps be done with Domain.Invariants (see below for more on Invariants): e.g. MyPredicate(x, y) => IsOfThisType(x).. Hmm..

        if (context.timelessDef() != null)
        {
            // This is something else that would require a change (addition) to the model to support. 
            // Something like a Sentence-collection-valued "Invariants" property on the Domain class
            // (which would cover both ":timeless" and ":axioms").
            throw new NotSupportedException(":timeless is not yet supported");
        }

        List<Action> actions = new();
        foreach (var structure in context.structureDef())
        {
            switch (structure)
            {
                case MinimalPDDLParser.ActionDefContext adc:
                    actions.Add(TransformAction(adc));
                    break;
                default:
                    throw new Exception("Unsupported domain structure type encountered");
            }
        }

        return new PddlDomain(name, actions);
    }

    private static Action TransformAction(MinimalPDDLParser.ActionDefContext context)
    {
        var identifier = context.NAME().Symbol.Text;

        // NB: Ignore the :parameters for now. Ultimately could validate it, but as in other
        // places, without typing, we can just infer parameters from variables present in precond and effect.

        var precondition = context.precondition != null
            ? GoalTransformation.Instance.Visit(context.precondition)
            : Goal.Empty;

        var effect = context.effect() != null
            ? EffectTransformation.Instance.Visit(context.effect())
            : Effect.Empty;

        return new Action(identifier, precondition, effect);
    }

    private static Problem TransformProblem(MinimalPDDLParser.ProblemContext context, Func<string, PddlDomain> getDomain)
    {
        // NB: Ignore the problem name - it's not in our model, and I'd be very reluctant to add it.
        // Problems don't need to know the label(s) used to refer to them - that's a serialisation concern.

        var domain = getDomain(context.domainName.Text);

        var requirementsFlags = context.requirementsDef()?._flags.Select(f => f.Text.ToLower());
        if (requirementsFlags != null && requirementsFlags.Any(f => !f.Equals(":strips")))
        {
            throw new NotSupportedException("Unsupported requirements flag(s) detected - only STRIPS is supported");
        }

        var initialState = TransformState(context.initDef()?.literalList());
        var goal = GoalTransformation.Instance.Visit(context.goalDef().goal());
        
        // TODO: add typing support - allow specification of predicate identifiers for "IsOfType" and factory for types
        // as domain elements. as it stands, there's not much to do with predicates. our model doesn't care about constants
        // on their own - and I'm now happy that it shouldn't. Existence is just a predicate, after all - just part of state
        // We *could* optionally allow for specification of a predicate identifier to attach to constants,
        // but wouldn't be much use if pddl doesn't use it. Obv becomes more relevant once
        //var constants = context.objectsDef()?.constantDecList()._elements.Select(e => new Constant(e.Text)) ?? Enumerable.Empty<Constant>();

        return new Problem(initialState, goal, domain.ActionSchemas);
    }

    private static IState TransformState(MinimalPDDLParser.LiteralListContext? context)
    {
        if (context != null)
        {
            // NB: PDDL init states allow literals rather than just predicates - that is, it does not
            // make the "closed-world" assumption. Our model has states as sets of predicates - implicitly
            // making the closed world assumption, so we only take the positive literals here.
            return new HashSetState(context._elements.Select(e => TransformLiteral(e)).Where(l => l.IsPositive).Select(l => l.Predicate));
        }
        else
        {
            return HashSetState.Empty;
        }
    }

    private static Literal TransformLiteral(MinimalPDDLParser.LiteralContext context)
    {
        return context switch
        {
            MinimalPDDLParser.PositiveLiteralContext plc => new Literal(TransformPredicate(plc.predicate())),
            MinimalPDDLParser.NegativeLiteralContext nlc => new Literal(TransformPredicate(nlc.predicate()), true),
            _ => throw new ArgumentException($"Unexpected LiteralContext type encountered: {context.GetType()}", nameof(context))
        };
    }

    private static Predicate TransformPredicate(MinimalPDDLParser.PredicateContext context)
    {
        var symbol = context.NAME().Symbol.Text;
        object identifier = symbol switch
        {
            "=" => EqualityIdentifier.Instance,
            _ => symbol
        };

        // should probably validate terms are in scope, given that PDDL expects stuff to be declared..
        var arguments = context.termList()._elements.Select<MinimalPDDLParser.TermContext, Term>(t => t switch
        {
            MinimalPDDLParser.ConstantTermContext ctc => new Function(ctc.NAME().Symbol.Text),
            MinimalPDDLParser.VariableTermContext vtc => new VariableReference(vtc.VARIABLE_NAME().Symbol.Text.TrimStart('?')),
            _ => throw new ArgumentException($"Unexpected TermContext type encountered: {context.GetType()}", nameof(context))
        });

        return new Predicate(identifier, arguments);
    }

    private class ThrowingErrorListener : BaseErrorListener
    {
        private ThrowingErrorListener() { }

        public static ThrowingErrorListener Instance { get; } = new ThrowingErrorListener();

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException("line " + line + ":" + charPositionInLine + " " + msg, "sentence", e);
        }
    }

    // Visitor that transforms from a syntax tree generated by ANTLR to a Goal instance
    private class GoalTransformation : MinimalPDDLBaseVisitor<Goal>
    {
        private GoalTransformation() { }

        public static GoalTransformation Instance { get; } = new GoalTransformation();

        public override Goal VisitLiteralGoal([NotNull] MinimalPDDLParser.LiteralGoalContext context)
        {
            return new Goal(TransformLiteral(context.literal()));
        }

        public override Goal VisitConjunctionGoal([NotNull] MinimalPDDLParser.ConjunctionGoalContext context)
        {
            // ugh, messy & inefficient - this visitor should perhaps be <IEnumerable<Literal>> instead..
            var elements = context.goal().Select(g => Visit(g)).SelectMany(g => g.Elements);
            return new Goal(elements);
        }
    }

    // Visitor that transforms from a syntax tree generated by ANTLR to an Effect instance
    private class EffectTransformation : MinimalPDDLBaseVisitor<Effect>
    {
        private EffectTransformation() { }

        public static EffectTransformation Instance { get; } = new EffectTransformation();

        public override Effect VisitLiteralEffect([NotNull] MinimalPDDLParser.LiteralEffectContext context)
        {
            return new Effect(TransformLiteral(context.literal()));
        }

        public override Effect VisitConjunctionEffect([NotNull] MinimalPDDLParser.ConjunctionEffectContext context)
        {
            // ugh, messy & inefficient - this visitor should perhaps be <IEnumerable<Literal>> instead..
            var elements = context.effect().Select(g => Visit(g)).SelectMany(g => g.Elements);
            return new Effect(elements);
        }
    }
}
