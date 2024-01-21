﻿// Copyright 2022-2023 Simon Condon
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

namespace SCClassicalPlanning.Planning;

/// <summary>
/// Helpful extension methods for <see cref="IPlanningTask"/> instances.
/// </summary>
public static class IPlanningTaskExtensions
{
    /// <summary>
    /// Executes a planning task to completion.
    /// </summary>
    /// <param name="planningTask">The planning task to execute.</param>
    /// <returns>The result of the planning task.</returns>
    public static Plan Execute(this IPlanningTask planningTask)
    {
        return planningTask.ExecuteAsync().GetAwaiter().GetResult();
    }
}
