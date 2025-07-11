﻿using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains.AsCode;

/// <summary>
/// The "Blocks World" example from §10.1.3 of "Artificial Intelligence: A Modern Approach".
/// </summary>
public static class BlocksWorldDomain
{
    static BlocksWorldDomain()
    {
        VariableDeclaration block = new(nameof(block));
        VariableDeclaration from = new(nameof(from));
        VariableDeclaration toBlock = new(nameof(toBlock));

        ActionSchemas = new[]
        {
            Move(block, from, toBlock),
            MoveToTable(block, from),
        }.AsQueryable();

        Function blockA = new(nameof(blockA));
        Function blockB = new(nameof(blockB));
        Function blockC = new(nameof(blockC));

        ExampleProblem = MakeProblem(
            initialState: new HashSetState(
                Equal(Table, Table)
                & Block(blockA)
                & Equal(blockA, blockA)
                & Block(blockB)
                & Equal(blockB, blockB)
                & Block(blockC)
                & Equal(blockC, blockC)
                & On(blockA, Table)
                & On(blockB, Table)
                & On(blockC, blockA)
                & Clear(blockB)
                & Clear(blockC)),
            endGoal: new(
                On(blockA, blockB)
                & On(blockB, blockC)));

        Function blockD = new(nameof(blockD));
        Function blockE = new(nameof(blockE));

        LargeExampleProblem = MakeProblem(
            initialState: new HashSetState(
                Equal(Table, Table)
                & Block(blockA)
                & Equal(blockA, blockA)
                & Block(blockB)
                & Equal(blockB, blockB)
                & Block(blockC)
                & Equal(blockC, blockC)
                & Block(blockD)
                & Equal(blockD, blockD)
                & Block(blockE)
                & Equal(blockE, blockE)
                & On(blockA, Table)
                & On(blockB, Table)
                & On(blockC, blockA)
                & On(blockD, blockB)
                & On(blockE, Table)
                & Clear(blockD)
                & Clear(blockE)
                & Clear(blockC)),
            endGoal: new(
                On(blockA, blockB)
                & On(blockB, blockC)
                & On(blockC, blockD)
                & On(blockD, blockE)));

        UnsolvableExampleProblem = MakeProblem(
            initialState: new HashSetState(
                Equal(Table, Table)
                & Block(blockA)
                & Equal(blockA, blockA)
                & Block(blockB)
                & Equal(blockB, blockB)
                & Block(blockC)
                & Equal(blockC, blockC)
                & On(blockA, Table)
                & On(blockB, Table)
                & On(blockC, Table)
                & Clear(blockA)
                & Clear(blockB)
                & Clear(blockC)),
            endGoal: new(
                On(blockA, blockB)
                & On(blockB, blockC)
                & On(blockC, blockA)));
    }

    /// <summary>
    /// Gets an instance of the customary example problem in this domain.
    /// Consists of three blocks.
    /// In the initial state, blocks A and B are on the table and block C is on top of block A.
    /// The goal is to get block B on top of block C, and block A on top of block B.
    /// </summary>
    public static Problem ExampleProblem { get; }

    /// <summary>
    /// Gets an instance of an example problem in this domain that is larger than the customary 3-block <see cref="ExampleProblem"/>.
    /// Consists of five blocks.
    /// </summary>
    public static Problem LargeExampleProblem { get; }

    /// <summary>
    /// Gets an instance of an example problem in this domain that is unsolvable.
    /// Consists of three blocks.
    /// In the initial state, all three blocks are on the table.
    /// The goal is to get block A on top of block B, block B on top of block C, and block C on top of block A.
    /// Which is of course impossible.
    /// </summary>
    public static Problem UnsolvableExampleProblem { get; }

    /// <summary>
    /// Gets the actions that are available in the "Blocks World" domain.
    /// </summary>
    public static IQueryable<Action> ActionSchemas { get; }

    public static Function Table { get; } = new(nameof(Table));

    public static OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
    public static OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
    public static OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
    public static OperablePredicate Equal(Term x, Term y) => new Predicate(EqualityIdentifier.Instance, x, y);

    public static Action Move(Term block, Term from, Term toBlock) => new OperableAction(
        identifier: nameof(Move),
        precondition:
            Block(block)
            & Block(toBlock)
            & !Equal(block, from)
            & !Equal(block, toBlock)
            & !Equal(from, toBlock)
            & On(block, from)
            & Clear(block)
            & Clear(toBlock),
        effect:
            On(block, toBlock)
            & Clear(from)
            & !On(block, from)
            & !Clear(toBlock));

    public static Action MoveToTable(Term block, Term from) => new OperableAction(
        identifier: nameof(MoveToTable),
        precondition:
            Block(block)
            & !Equal(block, from)
            & !Equal(from, Table)
            & On(block, from)
            & Clear(block),
        effect:
            On(block, Table)
            & Clear(from)
            & !On(block, from));

    /// <summary>
    /// Creates a new <see cref="Problem"/> instance that refers to this domain.
    /// </summary>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="endGoal">The end goal of the problem.</param>
    /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
    public static Problem MakeProblem(IState initialState, Goal endGoal) => new(initialState, endGoal, ActionSchemas);
}