﻿// Copyright 2022-2024 Simon Condon
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
using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation;

/// <summary>
/// Base class for recursive transformations of <see cref="Effect"/> instances to other <see cref="Effect"/> instances.
/// </summary>
public abstract class RecursiveEffectTransformation
{
    /// <summary>
    /// <para>
    /// Applies this transformation to a <see cref="Effect"/> instance.
    /// </para>
    /// <para>
    /// The default implementation returns a <see cref="Effect"/> with an element list that is the result of calling <see cref="ApplyTo(Literal)"/> on all of the existing elements.
    /// </para>
    /// </summary>
    /// <param name="effect">The effect to transform.</param>
    /// <returns>The transformed effect.</returns>
    public virtual Effect ApplyTo(Effect effect)
    {
        var isChanged = false;

        var elements = effect.Elements.Select(a =>
        {
            var transformed = ApplyTo(a);
            if (transformed != a)
            {
                isChanged = true;
            }
            return transformed;
        }).ToList();

        if (isChanged)
        {
            return new Effect(elements);
        }

        return effect;
    }

    /// <summary>
    /// Applies this transformation to a <see cref="Literal"/> instance. 
    /// The default implementation returns a <see cref="Literal"/> with the same positivity as te existing literal and a predicate that it the result of calling <see cref="ApplyTo(Predicate)"/> on the existing predicate.
    /// </summary>
    /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
    /// <returns>The transformed predicate.</returns>
    public virtual Literal ApplyTo(Literal literal)
    {
        var predicate = ApplyTo(literal.Predicate);
        if (predicate != literal.Predicate)
        {
            return new Literal(predicate, literal.IsNegated);
        }

        return literal;
    }

    /// <summary>
    /// Applies this transformation to a <see cref="Predicate"/> instance. 
    /// The default implementation returns a <see cref="Predicate"/> with the same Symbol and with an argument list that is the result of calling <see cref="ApplyTo(Term)"/> on all of the existing arguments.
    /// </summary>
    /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
    /// <returns>The transformed literal.</returns>
    public virtual Predicate ApplyTo(Predicate predicate)
    {
        var isChanged = false;
        var transformed = new Term[predicate.Arguments.Count];

        for (int i = 0; i < predicate.Arguments.Count; i++)
        {
            transformed[i] = ApplyTo(predicate.Arguments[i]);

            if (transformed[i] != predicate.Arguments[i])
            {
                isChanged = true;
            }
        }

        if (isChanged)
        {
            return new Predicate(predicate.Identifier, transformed);
        }

        return predicate;
    }

    /// <summary>
    /// Applies this transformation to a <see cref="Term"/> instance.
    /// The default implementation simply invokes the ApplyTo method appropriate to the type of the term.
    /// </summary>
    /// <param name="term">The term to visit.</param>
    /// <returns>The transformed term.</returns>
    public virtual Term ApplyTo(Term term)
    {
        return term switch
        {
            VariableReference variable => ApplyTo(variable),
            Function function => ApplyTo(function),
            _ => throw new ArgumentException($"Unsupported Term type '{term.GetType()}'", nameof(term))
        };
    }

    /// <summary>
    /// Applies this transformation to a <see cref="Function"/> instance.
    /// The default implementation returns a <see cref="Function"/> with the same Symbol and with an argument list that is the result of calling <see cref="ApplyTo(Term)"/> on each of the existing arguments.
    /// </summary>
    /// <param name="function">The function to transform.</param>
    /// <returns>The transformed term.</returns>
    public virtual Term ApplyTo(Function function)
    {
        var isChanged = false;
        var transformed = new Term[function.Arguments.Count];

        for (int i = 0; i < function.Arguments.Count; i++)
        {
            transformed[i] = ApplyTo(function.Arguments[i]);

            if (transformed[i] != function.Arguments[i])
            {
                isChanged = true;
            }
        }

        if (isChanged)
        {
            return new Function(function.Identifier, transformed);
        }

        return function;
    }

    /// <summary>
    /// Applies this transformation to a <see cref="VariableReference"/> instance.
    /// The default implementation returns a <see cref="VariableReference"/> referring to the variable that is the result of calling <see cref="ApplyTo(VariableDeclaration)"/> on the current declaration.
    /// </summary>
    /// <param name="variable">The variable reference to transform.</param>
    /// <returns>The transformed term.</returns>
    public virtual Term ApplyTo(VariableReference variable)
    {
        var variableDeclaration = ApplyTo(variable.Declaration);
        if (variableDeclaration != variable.Declaration)
        {
            return new VariableReference(variableDeclaration);
        }

        return variable;
    }

    /// <summary>
    /// Applies this transformation to a <see cref="VariableDeclaration"/> instance.
    /// The default implementation simply returns the passed value.
    /// </summary>
    /// <param name="variableDeclaration">The <see cref="VariableDeclaration"/> instance to transform.</param>
    /// <returns>The transformed <see cref="VariableReference"/> declaration.</returns>
    public virtual VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
    {
        return variableDeclaration;
    }
}
