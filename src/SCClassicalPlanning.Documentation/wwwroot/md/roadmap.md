# Roadmap

As mentioned in the package overview, the main goal here is learning and experimentation - both for myself and others.
I certainly understand that there's a fallacy here (we learn by doing - so the fact that I've learned by doing this doesn't mean that others will learn by consuming it), but I would hope that others will find it interesting, even if not useful.

There is a secondary goal of providing a starting point for extension to something properly useful - hence the attention that I'll pay to finding useful abstractions before we hit v1.
There is of course potential for this to conflict with the main goal - by abstracting, we add complexity, which can make things harder to learn. I'll try my best to strike a good compromise when it happens.

## Pre-V1

Stuff I want to do before ticking the major version number to 1:

- [x] Forward search 'works':
  - [x] the tests pass in a timely fashion with the example domains.
- [ ] Backward search 'works':
  - [x] The tests pass in a timely fashion with the example domains.
  - [ ] Backward search can work with goals with variables in them.
- [ ] We have simple implementations of state space search heuristics:
    - [x] one that could be described as using "ignore preconditions"
    - [ ] one that could be described as using "ignore delete lists"
    - [x] perhaps some others
- [x] We have a simple PlanningGraph implementation
- [ ] We have a simple GraphPlan implementation
- [ ] I've at least considered all of the existing TODOs.
    - [ ] Most notably the ones around State abstraction and state indexing - so its at least conceivable that someone could extend it to be workable with large (secondary-storage requiring) problems.
    - [ ] Also want to allow planners to open up the planning process. E.g. allowing consumers to see the search in state space searching, and the planning graph in graphplan. Perhaps even allow for them to be created step-by-step. Yes this is overhead, but given the purpose of the lib.. Likely accomplished by having IPlanner return a PlanningTask rather than a Task<Plan>

## Post-V1

Honestly not sure - might call it 'done' at that point, might continue playing with it.
Will certainly be willing to talk to any interested parties about it. Time will tell.

* Strong naming and package signing: only if it becomes popular or people ask for it. Certificates cost money..
