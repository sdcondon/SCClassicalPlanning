// Copyright 2022-2023 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning;

/// <summary>
/// Implementation of <see cref="IDomain"/> that just stores all of its elements in hash sets.
/// </summary>
public class HashSetDomain : IDomain
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetDomain"/> class.
    /// </summary>
    /// <param name="actions">The set of actions that exist within the domain.</param>
    public HashSetDomain(IEnumerable<Action> actions) // TODO: perhaps allow for the specification of additional constants?
    {
        Actions = actions.ToImmutableHashSet();

        var predicates = new HashSet<Predicate>();
        foreach (var action in actions)
        {
            PredicateFinder.Instance.Visit(action, predicates);
        }

        Predicates = predicates.ToImmutableHashSet();

        var constants = new HashSet<Constant>();
        foreach (var action in actions)
        {
            ConstantFinder.Instance.Visit(action, constants);
        }

        Constants = constants.ToImmutableHashSet();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetDomain"/> class.
    /// </summary>
    /// <param name="actions">The set of actions that exist within the domain.</param>
    public HashSetDomain(params Action[] actions) : this((IList<Action>)actions) { }

    /// <inheritdoc />
    IQueryable<Action> IDomain.Actions => Actions.AsQueryable();

    /// <inheritdoc />
    IQueryable<Predicate> IDomain.Predicates => Predicates.AsQueryable();

    /// <inheritdoc />
    IQueryable<Constant> IDomain.Constants => Constants.AsQueryable();

    /// <summary>
    /// Gets the set of actions that exist within the domain.
    /// </summary>
    public ImmutableHashSet<Action> Actions { get; }

    /// <summary>
    /// Gets the set of predicates that exist within the domain.
    /// </summary>
    public ImmutableHashSet<Predicate> Predicates { get; }

    /// <summary>
    /// Gets the set of constants that exist within the domain
    /// </summary>
    public ImmutableHashSet<Constant> Constants { get; }

    private class PredicateFinder : RecursiveActionVisitor<HashSet<Predicate>>
    {
        public static PredicateFinder Instance { get; } = new();

        public override void Visit(Predicate predicate, HashSet<Predicate> predicates)
        {
            // Standardise the arguments so that we unify all occurences of the 'same' predicate (with the same symbol and same number of arguments)
            // Yeah, could be safer, but realistically not going to have a predicate with this many parameters..
            // Ultimately, it might be a nice quality-of-life improvement to keep variable names as-is if its appropriate (e.g. if its the same
            // in all copies of this predicate) - but can come back to that.
            var standardisedParameters = Enumerable.Range(0, predicate.Arguments.Count).Select(i => new VariableReference(((char)('A' + i)).ToString())).ToArray();
            predicates.Add(new Predicate(predicate.Identifier, standardisedParameters));
        }
    }

    private class ConstantFinder : RecursiveActionVisitor<HashSet<Constant>>
    {
        public static ConstantFinder Instance { get; } = new();

        public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
    }
}
