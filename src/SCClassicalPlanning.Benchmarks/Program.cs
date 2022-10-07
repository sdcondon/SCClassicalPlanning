using SCClassicalPlanning; // ..for Action, Domain etc
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCClassicalPlanning.Planning.StateSpaceSearch;
using SCClassicalPlanning.Planning;
using SCFirstOrderLogic; // ..for Constant and Term
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory; // ..for OperablePredicate and single-letter VariableDeclarations
using Action = SCClassicalPlanning.Action; // Unfortunate clash with System.Action. I'd rather not rename, but.. we'll see
using System.Text;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;

// First, we need to create all the elements of the domain

// Our domain refers to a single constant - the table on which our blocks are placed:
Constant Table = new(nameof(Table));

// Our domain defines four predicates (facts about zero or more elements of the domain that can either hold true or not):
// As mentioned in the user guide for SCFirstOrderLogic, creating helper methods for your predicates is highly recommended.
// Note that we're using OperablePredicate here. Its not required, but makes everything nice and succinct because it
// means we can use & and ! to combine them. See SCFirstOrderLogic docs for details.
OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
OperablePredicate Equal(Term x, Term y) => new Predicate(nameof(Equal), x, y);

// Now declare some variables for use in our action schemas:
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

// The other action of our domain.
// 'moveToTable' is separate from 'moveToBlock' because of the different behaviour of table and block w.r.t being Clear or not.
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
// Specifically, what actions are available:
var domain = new Domain(moveToBlock, moveToTable);

Constant blockA = new(nameof(blockA));
Constant blockB = new(nameof(blockB));
Constant blockC = new(nameof(blockC));
Constant blockD = new(nameof(blockD));
Constant blockE = new(nameof(blockE));

var problem = new Problem(
    domain: domain,
    initialState: new(
        Block(blockA)
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
    goal: new(
        On(blockA, blockB)
        & On(blockB, blockC)
        & On(blockC, blockD)
        & On(blockD, blockE)));

// First instantiate a planner, specifying a heuristic to use (a delegate that estimates the
// number of actions it will take to get from a given state to a state that satisfies a given goal).
// NB: the only heuristic implemented so far is a super-simple one that just counts the differences
// between the current state and the goal. That's obviously not going to cut it for most problems,
// but suffices for the very simple problem we are trying to solve here:
var planner = new ForwardStateSpaceSearch(ElementDifferenceCount.EstimateCost);

// Tell the planner to create a plan for our problem:
var plan = planner.CreatePlanAsync(problem).GetAwaiter().GetResult(); // or obviously just await.. if we're in an async method

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