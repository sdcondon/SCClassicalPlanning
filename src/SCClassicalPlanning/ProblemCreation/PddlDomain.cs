// Copyright 2022-2024 Simon Condon
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
namespace SCClassicalPlanning.ProblemCreation;

/// <summary>
/// Represents a parsed PDDL domain.
/// </summary>
public class PddlDomain
{
    /// <summary>
    /// Initialises a new instance of the <see cref="PddlDomain"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="actions"></param>
    public PddlDomain(string name, IEnumerable<Action> actions)
    {
        Name = name;
        ActionSchemas = actions.AsQueryable();
    }

    /// <summary>
    /// Gets the name of the domain.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the actions that are available in problems of this domain.
    /// </summary>
    public IQueryable<Action> ActionSchemas { get; }
}
