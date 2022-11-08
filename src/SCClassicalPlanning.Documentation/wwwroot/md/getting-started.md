# Getting Started

Before we get started, two important notes:

* **NB #1:** This guide makes no attempt to explain classical planning; it just explains how to use this library to invoke some classical planning algorithms.
It is however worth noting that particular care has been taken with the type and member documentation - you *might* be able to intuit quite a lot just by combining this guide with those docs.
When all is said and done, classical planning isn't particularly complicated - conceptually at least.
* **NB #2:** Classical planning (as we define it here, at least) is built on top of first-order logic - classical planning elements (goals, effects etc) include elements of first order logic (notably, literals and predicates).
This library uses SCFirstOrderLogic as its model for this rather than creating its own.
As such, it *might* be useful to be passingly familiar with [SCFirstOrderLogic](https://sdcondon.net/SCFirstOrderLogic) before tackling this.

## Defining Problems

The first challenge is to define the planning problem to be solved.
In this section, we use the ['blocks world'](https://en.wikipedia.org/wiki/Blocks_world) domain that is a commonly used example (and indeed inspires the icon for this package!).

### Defining Problems as Code

```
using SCClassicalPlanning; // for Goal, Effect, Action, Domain, Problem, State
using SCFirstOrderLogic; // for Constant, Term, Predicate, VariableDeclaration, EqualitySymbol
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory; // for OperablePredicate
using Action = SCClassicalPlanning.Action; // an unfortunate clash with System.Action. I'd rather not rename it..

// First, we need to create everything that our domain definition will refer to.

// Our domain refers to a single constant - the table on which our blocks are placed:
Constant Table = new(nameof(Table));

// Our domain defines four predicates (essentially, facts about zero or more elements of the domain that,
// in any given state, are either true or not). As mentioned in the user guide for SCFirstOrderLogic, creating
// helper methods for your predicates, as we do below, is highly recommended.
// NB #1: note that we're using OperablePredicate here. Its not required, but makes everything nice and succinct because
// it means we can use & and ! to combine them. See the SCFirstOrderLogic docs for details.
// NB #2: Also note the use of EqualitySymbol.Instance here to identify the equality predicate. While at present there's
// no special handling of equality in any of the algorithms (in here or in SCFirstOrderLogic), its worth using this anyway
// - even if only for future-proofing.
OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
OperablePredicate Equal(Term x, Term y) => new Predicate(EqualitySymbol.Instance, x, y);

// Now declare some variables for use in our action schemas:
VariableDeclaration block = new(nameof(block));
VariableDeclaration from = new(nameof(from));
VariableDeclaration toBlock = new(nameof(toBlock));

// Our first action schema - 'moveToBlock'.
// This action moves a block from its current location to on top of a block:
Action moveToBlock = new(
    identifier: nameof(moveToBlock),
    precondition: new Goal(
        On(block, from)
        & Clear(block)
        & Clear(toBlock)
        & Block(block)
        & Block(toBlock)
        & !Equal(block, from)
        & !Equal(block, toBlock)
        & !Equal(from, toBlock)),
    effect: new Effect(
        On(block, toBlock)
        & Clear(from)
        & !On(block, from)
        & !Clear(toBlock)));

// The other action of our domain - 'moveToTable'.
// Separate from 'moveToBlock' because of the different behaviour of table and block w.r.t being Clear or not.
Action moveToTable = new(
    identifier: nameof(moveToTable),
    precondition: new Goal(
        On(block, from)
        & Clear(block)
        & Block(block)
        & !Equal(block, from)),
    effect: new Effect(
        On(block, Table)
        & Clear(from)
        & !On(block, from)));

// Now we are finally ready to declare our domain.
// A domain defines the common aspects of all problems that occur within it.
// Minimally, what actions are available:
var domain = new Domain(moveToBlock, moveToTable);

// Finally, we can declare our problem.
// Problems exist in a given domain, and consist of an initial state, an end goal, and a collection of objects that exist.
Constant blockA = new(nameof(blockA));
Constant blockB = new(nameof(blockB));
Constant blockC = new(nameof(blockC));

var problem = new Problem(
    domain: domain,
    initialState: new State(
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
    goal: new Goal(
        On(blockA, blockB)
        & On(blockB, blockC)));
```

### (Not) Defining Problems using a PDDL-like Grammar

It would be great to be able to point a parser at some text containing a problem defined in (an appropriate subset of) PDDL.
I haven't gotten around to that just yet, though..

## Solving Problems

Once you have a problem, the types in the `SCClassicalPlanning.Planning` namespace can be used to try to create a plan to solve it.

### Using Forward State Space Search

```
using SCClassicalPlanning.Planning; // for PlanFormatter and CreatePlan extension method (plan creation is async by default)
using SCClassicalPlanning.Planning.StateSpaceSearch; // for ForwardStateSpaceSearch
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics; // for UnsatisfiedGoalCount

// First instantiate a ForwardStateSpaceSearch, specifying a heuristic to use (an object that
// estimates the number of actions it will take to get from a given state to a state that satisfies
// a given goal). You can create a heuristic specific to the domain, or you can use a
// generic one provided by the library. Here we use one provided by the library.
// NB #1: both the forward and backward state space search classes have constructor overloads
// that also include a parameter for a delegate to compute the "cost" of an action. Potentially useful if 
// it makes sense for a custom heuristic to consider some actions more costly than others.
// NB #2: here we use the UnsatisfiedGoalCount heuristic - which just counts the number of elements of the
// goal that are not satisfied in th current state. It's totally useless for backward searches (because
// it can't rule out unsatisfiable goals) and isn't that great for most forward searches either
// (note it's not admissable, so can give non-optimal plans). However, it suffices for the very simple
// problem we are trying to solve here:
var planner = new ForwardStateSpaceSearch(new UnsatisfiedGoalCount());

// Tell the planner to create a plan for our problem:
var plan = planner.CreatePlan(problem);

// Now let's verify that applying the plan results in a state that satisfies the goal,
// printing out the actions included in the plan in the process:
var planFormatter = new PlanFormatter(problem.Domain);
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
}

Console.WriteLine($"Goal satisfied: {state.Satisfies(problem.Goal)}!");
```

### Using Backward State Space Search

As above, but using `var planner = new BackwardStateSpaceSearch(heuristic)`, where heuristic is an `IHeuristic`.
As mentioned above, UnsatisfiedGoalCount is useless for this. Either create one specifically for your
domain, or wait until I implement some more generic ones (working on a precondition-ignoring greedy set cover at the time
of writing).

### Using Graphplan

*Not yet!*
