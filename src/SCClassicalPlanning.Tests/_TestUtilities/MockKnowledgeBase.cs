using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;

namespace SCClassicalPlanning._TestUtilities
{
    /// <summary>
    /// Mock <see cref="IKnowledgeBase"/> implementation that can only provide canned responses.
    /// </summary>
    internal class MockKnowledgeBase : IKnowledgeBase
    {
        private readonly Func<Sentence, bool> responseCallback;

        public MockKnowledgeBase(Dictionary<Sentence, bool> responses)
            : this(s => responses[s])
        {
        }

        public MockKnowledgeBase(Func<Sentence, bool> responseCallback)
        {
            this.responseCallback = responseCallback;
        }

        public Task<IQuery> CreateQueryAsync(Sentence query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IQuery)new Query(responseCallback(query)));
        }

        public Task TellAsync(Sentence sentence, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private class Query : IQuery
        {
            private readonly bool result;

            public Query(bool result) => this.result = result;

            public bool IsComplete { get; private set; }

            public bool Result => IsComplete ? result : throw new InvalidOperationException();

            public void Dispose() {}

            public Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
            {
                IsComplete = true;
                return Task.FromResult(result);
            }
        }
    }
}
