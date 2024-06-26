﻿using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains.AsCode;

/// <summary>
/// <para>
/// Static logic pertaining to a very simple domain, used for tests.
/// </para>
/// <para>
/// The idea is that we have some kind of (global) container that objects (the elements of the domain) can be added to and removed from.
/// </para>
/// </summary>
public static class ContainerDomain
{
    /// <summary>
    /// Gets the actions that are available in the "Container" domain.
    /// </summary>
    public static IQueryable<Action> ActionSchemas { get; } = new[]
    {
        Add(A),
        Remove(R),
        Swap(R, A),
    }.AsQueryable();

    /// <summary>
    /// Constructs an <see cref="OperablePredicate"/> instance for indicating that a given domain element is present within the container.
    /// </summary>
    /// <param name="element">A term that describes the domain element.</param>
    /// <returns>A new <see cref="OperablePredicate"/> instance for indicating that a given domain element is present within the container.</returns>
    public static OperablePredicate IsPresent(Term element) => new Predicate(nameof(IsPresent), element);

    /// <summary>
    /// Constructs an <see cref="Action"/> instance that describes the addition of a given domain element to the container.
    /// </summary>
    /// <param name="element">A term that describes the domain element.</param>
    /// <returns>A new <see cref="Action"/> instance that describes the addition of a given domain element to the container.</returns>
    public static Action Add(Term @object) => new OperableAction(
        identifier: nameof(Add),
        precondition: !IsPresent(@object),
        effect: IsPresent(@object));

    /// <summary>
    /// Constructs an <see cref="Action"/> instance that describes the removal of a given domain element from the container.
    /// </summary>
    /// <param name="element">A term that describes the domain element.</param>
    /// <returns>A new <see cref="Action"/> instance that describes the removal of a given domain element from the container.</returns>
    public static Action Remove(Term @object) => new OperableAction(
        identifier: nameof(Remove),
        precondition: IsPresent(@object),
        effect: !IsPresent(@object));

    /// <summary>
    /// Constructs an <see cref="Action"/> instance that describes the swapping of a given domain element in the container for another.
    /// </summary>
    /// <param name="remove">A term that describes the removed domain element.</param>
    /// <param name="add">A term that describes the added domain element.</param>
    /// <returns>A new <see cref="Action"/> instance that describes the swapping of a given domain element in the container for another.</returns>
    public static Action Swap(Term remove, Term add) => new OperableAction(
        identifier: nameof(Swap),
        precondition: IsPresent(remove) & !IsPresent(add),
        effect: !IsPresent(remove) & IsPresent(add));

    /// <summary>
    /// Creates a new <see cref="Problem"/> instance that refers to this domain.
    /// </summary>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="endGoal">The end goal of the problem.</param>
    /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
    public static Problem MakeProblem(IState initialState, Goal endGoal) => new(initialState, endGoal, ActionSchemas);
}