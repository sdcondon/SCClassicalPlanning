# Library Overview

Here is a quick overview of the namespaces found within this library. Reading this should give you some helpful context for diving a little deeper:

* **`SCClassicalPlanning`:** the root namespace contains classes representing individual elements of planning problems (problems, domains, states, actions, goals and effects).
  * **`Planning`:** intended as the top-level namespace for actual planning algorithms. Directly contains `IPlanner`, an interface for types capable of creating plans to solve problems.
    * **`GraphPlan`:** contains an implementation of `IPlanner` that uses the GraphPlan algorithm - as well as some supporting types (notably, a class for planning graphs).
    * **`StateAndGoalSpace`:** contains some very simple `IPlanner` implementations that search state- or goal-space to create plans.
      * **`CostStrategies`:** contains cost strategy implementations for use by search planners.
    * **`Utilities`:** contains useful common logic for planners to make use of. For example, logic for the retrieval of applicable and relevant actions.
  * **`ProblemCreation`:** contains types that assist with the creation of problems and their constituent elements.
  * **`ProblemManipulation`:** contains types to assist with the manipulation of problems and their constituent elements.

For a full type and member listing, the recommendation is to use the [FuGet package explorer](https://www.fuget.org/packages/SCClassicalPlanning/) - though going through [getting started](getting-started.md) first is probably a good idea, if you haven't already.