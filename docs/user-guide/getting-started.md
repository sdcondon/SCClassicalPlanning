# Getting Started

Before we get started, two important notes:

* **NB #1:** This guide makes no attempt to explain classical planning; it just explains how to use this library to invoke some classical planning algorithms.
It is however worth noting that particular care has been taken with the type and member documentation - you *might* be able to intuit quite a lot just by combining this guide with those docs.
* **NB #2:** Classical planning (as we define it here, at least) is built on top of first-order logic - classical planning elements (goals, effects etc) include elements of first order logic (notably, literals and predicates).
This library uses SCFirstOrderLogic as its model for this rather than creating its own.
As such, it *might* be useful to be passingly familiar with [SCFirstOrderLogic](https://github.com/sdcondon/SCFirstOrderLogic) before tackling this.

## Defining Problems

The first challenge is to define the planning problem to be solved. In this section, we use the "blocks world" domain that is apparently fairly common in planning circles..

### Defining Problems as Code

```csharp
using SCClassicalPlanning; // ..for Action, Domain etc
using SCFirstOrderLogic; // ..for Constant and Term
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory; // ..for OperablePredicate and single-letter VariableDeclarations
using Action = SCClassicalPlanning.Action; // Unfortunate clash with System.Action. I'd rather not rename, but.. we'll see

// First, we need to create all the elements of the domain

// Our domain refers to a single constant term - the table on which our blocks are placed:
Constant Table = new(nameof(Table));

// Our domain defines four predicates (facts about zero or more objects that can either hold true or not):
// As mentioned in the user guide for SCFirstOrderLogic, creating helper methods for your predicates is highly recommended.
// Note that we're using OperablePredicate here. Its not required, but makes everything nice and succinct because it
// means we can use & and ! to combine them. See SCFirstOrderLogic docs for details.
OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
OperablePredicate Equal(Term x, Term y) => new Predicate(nameof(Equal), x, y);

// Now declare some variable terms for use in our action schemas:
VariableDeclaration block = new(nameof(block));
VariableDeclaration from = new(nameof(from));
VariableDeclaration toBlock = new(nameof(toBlock));

// Our first action schema.
// The 'moveToBlock' action moves a block from its current location to on top of a block:
Action moveToBlock = new Action(
    identifier: nameof(moveToBlock),
    precondition: new(
        On(block, from)
        & Clear(block)
        & Clear(toBlock)
        & Block(block)
        & Block(toBlock)
        & !Equal(block, from)
        & !Equal(block, toBlock)
        & !Equal(from, toBlock)),
    effect: new(
        On(block, toBlock)
        & Clear(from)
        & !On(block, from)
        & !Clear(toBlock)));

// The other ation of our domain - 'moveToTable'.
// Separate from 'moveToBlock' because of the different behaviour of table and block w.r.t being Clear or not.
Action moveToTable = new Action(
    identifier: nameof(moveToTable),
    precondition: new(
        On(block, from)
        & Clear(block)
        & Block(block)
        & !Equal(block, from)),
    effect: new(
        On(block, Table)
        & Clear(from)
        & !On(block, from)));

// Now we are ready to declare our domain.
// A domain defines the common aspects of all problems that occur within it.
// Specifically, what predicates exist and what actions are available:
// (NB: I will almost certainly add a ctor overload at some point that allows
// the predicates to be inferred from the actions).
var domain = new Domain(
    predicates: new Predicate[]
    {
        Block(B),
        On(B, S),
        Clear(S),
        Equal(X, Y),
    },
    actions: new Action[]
    {
        moveToBlock,
        moveToTable
    });

// Now we declare a few more constants.
// These ones are specific to the problem we want to solve:
Constant blockA = new(nameof(blockA));
Constant blockB = new(nameof(blockB));
Constant blockC = new(nameof(blockC));

// Finally, we can declare our problem.
// Problems exist in a given domain, and consist of an initial state, an end goal, and a collection of objects that exist.
// Note that here we have used a constructor overload without explicitly specifying what objects exist. This overload will
// assume the existing objects are precisely the Constants that are referred to by the initial state and goal.
var problem = new Problem(
    domain: domain,
    initialState: new(
        Block(blockA)
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
    goal: new(
        On(blockA, blockB)
        & On(blockB, blockC)));
```

### (Not) Defining Problems using a PDDL-like Grammar

It would be great to be able to point a parser at some text containing a problem defined in (an appropriate subset of) PDDL.
I haven't gotten around to that just yet, though..

## Solving Problems

Once you have a problem, the types in the `SCClassicalPlanning.Planning` namespace can be used to try to create a plan to solve it.

### Using Forward State Space Search

```csharp
using SCClassicalPlanning.Planning; // For PlanFormatter
using SCClassicalPlanning.Planning.StateSpaceSearch; // For the planner
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics; // For ElementDifferenceCount

// First instantiate a planner, specifying a heuristic to use (an delegate that estimates of
// the number of actions it will take to get from a given state to a state that satisfies a given goal).
// NB: the only heuristic implemented so far is a super-simple one that just counts the differences
// between the current state and the goal. That's obviously not going to cut it in the real world,
// but suffices for the very simple problems we are trying to solve here:
var planner = new ForwardStateSpaceSearch(ElementDifferenceCount.EstimateCountOfActionsToGoal);

// Tell the planner to create a plan for our problem:
var plan = planner.CreatePlanAsync(problem).GetAwaiter().GetResult(); // or obviously just await.. if we're in an async method

// Verify that applying the plan results in a state that satisfies the goal,
// printing out the actions included in the plan and the intermediate state in the process.
// NB: output is obviously something that could be improved upon. In particular, I'm
// aware that it'd be nice to explictly state what the variables used in the action schema
// are mapped to at each stage. That's a TODO.
var planFormatter = new PlanFormatter();
var state = problem.InitialState;
foreach (var action in plan.Steps)
{
    Console.WriteLine(planFormatter.Format(action));

    if (!action.IsApplicableTo(state))
    {
        Console.WriteLine("Invalid plan of action - current action is not applicable in the current state");
        break;
    }

    state = action.ApplyTo(state);

    Console.WriteLine($"Current state: {planFormatter.Format(state)}");
    Console.WriteLine();
}

Console.WriteLine($"Goal satisfied: {state.Satisfies(problem.Goal)}!");
```

### Using Backward State Space Search

As above, but using `var planner = new BackwardStateSpaceSearch(ElementDifferenceCount.EstimateCountOfActionsToGoal)`.

### Using Graphplan

*Not yet!*
