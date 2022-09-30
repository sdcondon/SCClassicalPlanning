using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public static class StateSpaceSearchHeuristics
    {
        public static float IgnorePreconditions(State initial, Goal goal)
        {
            return 0;
        }

        public static float IgnoreDeleteLists(State initial, Goal goal)
        {
            return 0;
        }
    }
}
