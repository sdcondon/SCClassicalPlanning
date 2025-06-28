![SCClassicalPlanning Icon](src/SCClassicalPlanning-128.png)

# SCClassicalPlanning

[![NuGet version (SCClassicalPlanning)](https://img.shields.io/nuget/v/SCClassicalPlanning.svg?style=flat-square)](https://www.nuget.org/packages/SCClassicalPlanning/) 
[![NuGet downloads (SCClassicalPlanning)](https://img.shields.io/nuget/dt/SCClassicalPlanning.svg?style=flat-square)](https://www.nuget.org/packages/SCClassicalPlanning/) 
[![Commits since latest release](https://img.shields.io/github/commits-since/sdcondon/SCClassicalPlanning/0.11.0?style=flat-square)](https://github.com/sdcondon/SCClassicalPlanning/compare/latest-release...main) 

This repository contains the source code for the SCClassicalPlanning NuGet package - in addition to its tests, benchmarks, alternative implementations, example domains and documentation website.

## Package Documentation

For documentation of the package itself, see https://sdcondon.net/SCClassicalPlanning/.

## Source Documentation

I haven't written up any documentation of the source (e.g. repo overview, design discussion, compilation guidance…) - and likely won't unless someone else expresses an interest in contributing.
Once cloned, it should "just work" as far as compilation is concerned.
The only thing perhaps worthy of note is that it uses [Antlr4BuildTasks](https://github.com/kaby76/Antlr4BuildTasks) to invoke ANTLR to generate the PDDL-parsing code. ANTLR is a Java tool - something Antlr4BuildTasks handles by downloading the JRE to your system for you - see its docs for details.

## Issues and Contributions

I'm not really expecting anyone to want to get involved at this stage, but please feel free to do so.
I do keep an eye on the [issues](https://github.com/sdcondon/SCClassicalPlanning/issues) tab, and will add a CONTRIBUTING.md if anyone drops me a message expressing interest.
Do bear in mind that I have a very particular scope in mind for the library, though - see the [roadmap](https://sdcondon.net/SCClassicalPlanning/roadmap.md) for details.

## See Also

Like this? If so, you might also be interested in these:

* [SCFirstOrderLogic](https://github.com/sdcondon/SCFirstOrderLogic): Basic first-order logic implementations. Based on chapters 8 and 9 of "Artificial Intelligence: A Modern Approach". Depended on by this package (for its first-order logic model).
* [SCGraphTheory.Search](https://github.com/sdcondon/SCGraphTheory.Search): Graph theory search algorithms. Depended on by this package (for state space search).
* [AIMA Code - C#](https://github.com/aimacode/aima-csharp): I mention this only because I feel like I should. This is the "official" C# repository for "Artificial Intelligence: A Modern Approach" - and it is.. err.. not great.
* [My GitHub Profile](https://github.com/sdcondon): My profile README lists a whole bunch of stuff that I've made.
