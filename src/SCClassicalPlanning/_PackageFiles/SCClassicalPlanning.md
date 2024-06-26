# SCClassicalPlanning

Basic but fully functional and documented [classical planning](https://www.google.com/search?q=classical+planning) implementations.
Somewhat influenced by chapter 10 of _Artificial Intelligence: A Modern Approach_ (3rd Edition - [ISBN 978-1292153964](https://www.google.com/search?q=isbn+978-1292153964)).
Intended first and foremost to assist with learning and experimentation, but will (by the time v1 arrives) include extension points (and async support) so that it is at least conceivable that it could be (extended to be) useful in a production scenario.
Includes:

* A model for planning problems that consists of [Problem](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Problem.cs), [IState](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/IState.cs), [Goal](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Goal.cs), [Action](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Action.cs), and [Effect](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Effect.cs).
Note that while this is a smaller model than even the earliest versions of [PDDL](https://en.wikipedia.org/wiki/Planning_Domain_Definition_Language), it does go further than chapter 10 of AIaMA  - partly out of necessity (to create a coherent set of functionality), and partly to (partially!) "bridge the gap" between the rough ideas in this chapter and real, production-ready planners.
* A few basic planner implementations. One that uses state space search, one that uses goal space search, and one that uses Graphplan.

As mentioned above, the main goal here is for it to be a resource for learning and experimentation.
As such, care has been taken to include good type and member documentation, as well as [Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink) to allow for debugging - and explanatory inline comments in the source code where appropriate.

Full documentation can be found on the [documentation website](https://sdcondon.net/SCClassicalPlanning).