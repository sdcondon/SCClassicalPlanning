![SCClassicalPlanning Icon](src/SCClassicalPlanning.png)

# SCClassicalPlanning

This repository contains the source code for the SCClassicalPlanning NuGet package.

## Package Overview

Basic but fully functional and documented classical planning implementations.
Somewhat influenced by chapter 10 of _Artificial Intelligence: A Modern Approach_ (3rd Edition - [ISBN 978-1292153964](https://www.google.com/search?q=isbn+978-1292153964)).
Intended first and foremost to assist with learning and experimentation, but will (by the time v1 arrives) include extension points (and async support) so that it is at least conceivable that it could be (extended to be) useful in a production scenario.
Includes:

* A model for planning problems that consists of [Problem](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Problem.cs), [Domain](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Domain.cs), [State](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/State.cs), [Goal](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Goal.cs), [Action](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Action.cs), and [Effect](https://github.com/sdcondon/SCClassicalPlanning/blob/main/src/SCClassicalPlanning/Effect.cs).
Note that while this is a significantly smaller model than that capable of being expressed with even the earliest versions of [PDDL](https://en.wikipedia.org/wiki/Planning_Domain_Definition_Language), it does go further than chapter 10 of AIaMA  - partly out of necessity (to create a coherent set of functionality), and partly to (partially!) "bridge the gap" between the rough ideas in this chapter and real, production-ready planners.
It is perhaps worth noting that I do feel chapter 10 is one of the weaker chapters of the source material, simply because of the need to add quite a lot to it to create something coherent.
* A few basic planner implementations. One that uses forward state space search, one that uses backward state space search, and one that uses graphplan (NB: GraphPlan not there just yet - will appear prior to v1).

As mentioned above, the main goal here is for it to be a resource for learning and experimentation.
As such, care has been taken to include good type and member documentation, as well as [Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink) to allow for debugging - and explanatory inline comments in the source code where appropriate.

## Package Documentation

For the moment at least, documentation can be found in the docs folder of this repo. Specifically, we have:

* *Nothing, just yet!*

## Issues and Contributions

I'm not really expecting anyone to want to get involved at this stage (or indeed ever), but please feel free to do so.
I do keep a vague eye on the [issues](https://github.com/sdcondon/SCClassicalPlanning/issues) tab, and will add a CONTRIBUTING.md if anyone drops me a message expressing interest.
Do bear in mind that I have a very particular scope in mind for the library, though.
When we reach the point where I feel that it does a good job of its "learning and experimenting" remit and offers sufficient means of extension, I'll view it as done (and suggest that any extensions belong in their own packages).

## See Also

Like this? If so, it might also be worth taking a look at:

* [SCFirstOrderLogic](https://github.com/sdcondon/SCFirstOrderLogic): Basic first-order logic implementations. Based on chapters 8 and 9 of "Artificial Intelligence: A Modern Approach". Depended on by this package (for its first-order logic model).
* [SCGraphTheory.Search](https://github.com/sdcondon/SCGraphTheory.Search): Graph theory search algorithms. Depended on by this package (for state space search).
* [AIMA Code - C#](https://github.com/aimacode/aima-csharp): I mention this only because I feel like I should. This is the "official" C# repository for "Artificial Intelligence: A Modern Approach" - and it is utterly, irretrievably useless.