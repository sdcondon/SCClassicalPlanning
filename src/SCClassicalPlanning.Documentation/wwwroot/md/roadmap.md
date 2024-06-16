# Roadmap

As mentioned in the package overview, the main goal here is learning and experimentation.
As such, I will likely consider it "done" when I feel that this goal is met - when it becomes a substantial enough resource for people who are learning classical planning.

There is a secondary goal of providing a starting point for extension to something properly useful - hence the attention that I'll pay to finding useful abstractions before we hit v1.
There is of course potential for this to conflict with the main goal - by abstracting, we add complexity, which can make things harder to learn. I'll try my best to strike a good compromise when it happens.

## Pre-V1

Stuff I want to do before ticking the major version number to 1:

- [x] State space search works acceptably well:
  - [x] The example domain tests pass in a timely fashion.
- [ ] Goal space search works acceptably well:
  - [x] The example domain tests pass.
  - [ ] The example domain tests pass in a timely fashion.
  - [ ] Goal space search works in a 'lifted' fashion - retaining variables for as long as it can.
- [ ] Simple implementations of search heuristics:
    - [x] one that could be described as using "ignore preconditions"
    - [ ] one that could be described as using "ignore delete lists"
    - [x] perhaps some others
- [x] A simple PlanningGraph implementation
- [x] A simple GraphPlan implementation
- [x] Basic PDDL parsing
- [ ] I've at least considered all of the existing TODOs.
    - [x] Most notably the ones around State abstraction and state indexing - so its at least conceivable that someone could extend it to be workable with large (secondary storage requiring) problems.
    - [x] Also want to allow planners to open up the planning process. E.g. allowing consumers to see the search in state space searching, and the planning graph in graphplan. Perhaps even allow for them to be created step-by-step. Yes this is overhead, but given the purpose of the lib.. Likely accomplished by having IPlanner return a PlanningTask rather than a Task&lt;Plan&gt;

## Post-V1

Honestly not sure - might call it 'done' at that point, might continue playing with it. Time will tell.
