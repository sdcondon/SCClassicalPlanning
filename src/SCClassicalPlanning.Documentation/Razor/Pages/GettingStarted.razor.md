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
using SCClassicalPlanning; // for HashSetState, Goal, Action, Effect, Problem
using SCFirstOrderLogic; // for Function, Term, Predicate, VariableDeclaration, EqualityIdentifier
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory; // for OperablePredicate
using Action = SCClassicalPlanning.Action; // an unfortunate clash with System.Action. I'd rather not rename it..

// Our domain defines four predicates (essentially, facts about zero or more elements of the domain that,
// in any given state, are either true or not). As mentioned in the user guide for SCFirstOrderLogic, creating
// helper methods for your predicates, as we do below, is highly recommended to avoid repetition.
// NB #1: note that we're using OperablePredicate here. Its not required, but makes everything nice and succinct because
// it means we can use & and ! to combine them. See the SCFirstOrderLogic docs for details.
// NB #2: Also note the use of EqualityIdentifier.Instance here to identify the equality predicate. While at present there's
// no special handling of equality in any of the algorithms (in here or in SCFirstOrderLogic), its worth using this anyway
// - even if only for future-proofing.
OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
OperablePredicate Equal(Term x, Term y) => new Predicate(EqualityIdentifier.Instance, x, y);

// First, let's define the initial state of our problem. IState is an interface that represents a set of
// predicates that can be modified by applying Actions. For problems with large states, a state implementation
// that is backed by a external storage might be required, but here we have a small enough problem that just
// keeping everything in memory is fine. The library includes an implementation of IState called HashSetState
// that is intended for use in such scenarios.
Function table = new(nameof(table));
Function blockA = new(nameof(blockA));
Function blockB = new(nameof(blockB));
Function blockC = new(nameof(blockC));
HashSetState initialState = new(
    Block(blockA)
    & Equal(blockA, blockA)
    & Block(blockB)
    & Equal(blockB, blockB)
    & Block(blockC)
    & Equal(blockC, blockC)
    & On(blockA, table)
    & On(blockB, table)
    & On(blockC, blockA)
    & Clear(blockB)
    & Clear(blockC));

// Next, let's define the end goal of our problem. Goals are essentially sets of literals.
// Note that, unlike states, that is sets of *literals*, not predicates. This is so that we can require
// that certain predicates do *not* hold. In this case though, both of our goal's elements are positive.
Goal endGoal = new(
    On(blockA, blockB)
    & On(blockB, blockC));

// Thirdly, we define the schemas for the actions that are available to solve our problem.
// Action schemas can include variables - that planners can substitute appropriate constants for
// when creating a plan to solve the problem.
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

// Our second action schema - 'moveToTable'.
// Separate from 'moveToBlock' because of the different behaviour of table and block w.r.t being Clear or not.
Action moveToTable = new(
    identifier: nameof(moveToTable),
    precondition: new Goal(
        On(block, from)
        & Clear(block)
        & Block(block)
        & !Equal(block, from)),
    effect: new Effect(
        On(block, table)
        & Clear(from)
        & !On(block, from)));

// Note that problems require action schemas to be IQueryable<Action> - ultimately to allow
// for efficent action lookup when there are a large number of possible actions. This
// might change by the time this package reaches v1.0..
var actionSchemas = new[] { moveToBlock, moveToTable }.AsQueryable();

// Finally, all that is left to do is combine all of the above into a problem definition:
var problem = new Problem(initialState, endGoal, actionSchemas);
```

### Defining Problems with PDDL

As of v0.9, you can express problems as [PDDL](https://www.google.com/search?q=pddl), like this:

```
using SCClassicalPlanning.ProblemCreation; // For PddlParser

var domainPddl = 
@"(define (domain blocks-world)
    (:constants Table)
    (:action Move
        :parameters (?block ?from ?toBlock)
        :precondition (and
            (Block ?block)
            (Block ?toBlock)
            (not (= ?block ?from))
            (not (= ?block ?toBlock))
            (not (= ?from ?toBlock))
            (On ?block ?from)
            (Clear ?block)
            (Clear ?toBlock)
        )
        :effect (and
            (On ?block ?toBlock)
            (Clear ?from)
            (not (On ?block ?from))
            (not (Clear ?toBlock))
        )
    )
    (:action MoveToTable
        :parameters (?block ?from)
        :precondition (and
            (Block ?block)
            (not (= ?block ?from))
            (not (= ?from Table))
            (On ?block ?from)
            (Clear ?block)
        )
        :effect (and
            (On ?block Table)
            (Clear ?from)
            (not (On ?block ?from))
        )
    )
)";

var problemPddl = 
@"(define (problem sussman-anomaly)
    (:domain blocks-world)
    (:objects 
        blockA 
        blockB
        blockC
    )
    (:init
        (= Table Table)
        (Block blockA)
        (= blockA blockA)
        (Block blockB)
        (= blockB blockB)
        (Block blockC)
        (= blockC blockC)
        (On blockA Table)
        (On blockB Table)
        (On blockC blockA)
        (Clear blockB)
        (Clear blockC)
    )
    (:goal (and
        (On blockA blockB)
        (On blockB blockC)
    ))
)";

var problem = PddlParser.ParseProblem(problemPddl, domainPddl);
```

Notes:
* PDDL is a whole language, and no attempt to explain it will be made here - sorry!
The version of PDDL used is an absolutely minimal version of the earliest public version of the language (1.2).
Absolutely NO extensions (not even typing) are recognised at the moment - STRIPS only.
Have also omitted problem situations and length specs. While they aren't marked as a particular
extension by the spec, they are marked as deprecated by [this wiki](https://planning.wiki/ref/pddl/problem) (well, length spec isn't even mentioned).
That may just be due to incompleteness of this wiki, but
I'm taking every excuse I can to minimise this.

## Solving Problems

Once you have a problem, the types in the `SCClassicalPlanning.Planning` namespace can be used to try to create a plan to solve it.

### Using State Space Search

```
using SCClassicalPlanning.Planning; // for PlanFormatter and CreatePlan extension method (plan creation is async by default)
using SCClassicalPlanning.Planning.StateAndGoalSpace; // for StateSpaceAStarPlanner
using SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies; // for UnmetGoalCount

// First instantiate a StateSpaceAStarPlanner, specifying a strategy to use (an object that gives
// the cost of actions, and estimates the total cost of getting from a given state to a state
// that meets a given goal). You can create a strategy specific to the domain, or you can use a
// generic one provided by the library. Here we use one provided by the library.
// NB: here we use the UnmetGoalCount strategy - which (gives all actions a cost of 1 and) just 
// counts the number of elements of the goal that are not met in the current state. It's totally
// useless for goal space searches (because it can't rule out unmeetable goals) and isn't that great
// for most state space searches either (note it's not admissable, so can give non-optimal plans). 
// However, it suffices for the very simple problem we are trying to solve here:
var planner = new StateSpaceAStarPlanner(new UnmetGoalCount());

// Tell the planner to create a plan for our problem:
var plan = planner.CreatePlan(problem);

// Now let's verify that applying the plan results in a state that meets the goal,
// printing out the actions included in the plan in the process:
var planFormatter = new PlanFormatter(problem);
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

Console.WriteLine($"Goal met: {state.Meets(problem.EndGoal)}!");
```

### Using Goal Space Search

As above, but using `var planner = new GoalSpaceAStarPlanner(strategy);`, where strategy is an `IStrategy`.
As mentioned above, UnmetGoalCount is useless for this. Either create one specifically for your
domain, or wait until I implement some better generic ones.

### Using Graphplan

As above, but using `var planner = new GraphPlanPlanner();` (this type resides in SCClassicalPlanning.Planning.GraphPlan).
N.B. for the moment this planner has no configurability. 
This will likely change pre-v1 to add some configurability of the heuristic used during the backward search part.
