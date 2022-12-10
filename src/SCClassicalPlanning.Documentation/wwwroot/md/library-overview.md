# Library Overview

Here is a quick overview of the namespaces found within this library. Reading this should give you some helpful context for diving a little deeper:

* **`SCClassicalPlanning`:** the root namespace contains classes representing individual elements of planning problems (problems, domains, states, actions, goals and effects).
  * **`Planning`:** intended as the top-level namespace for actual planning algorithms. Directly contains `IPlanner`, an interface for types capable of creating plans to solve problems.
    * **`GraphPlan`:** Contains an implementation of `IPlanner` that uses the GraphPlan algorithm - as well as some supporting types (notably, a class for planning graphs).
    * **`Search`:** contains some very simple `IPlanner` implementations that search state- or goal-space to create plans.
      * **`Heuristics`:** contains heuristic implementations for use by search planners.
  * **`ProblemCreation`:** contains types that assist with the creation of problems and their constituent elements.
  * **`ProblemManipulation`:** contains types to assist with the manipulation of problems and their consituent elements.
