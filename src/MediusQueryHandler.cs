using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    /// <summary>
    /// Provides a base class for handling <see cref="IMediusQuery{TQueryResult}"/> operations.
    /// Query handlers execute logic that retrieves or computes data without modifying state,
    /// returning a result of type <typeparamref name="TQueryResult"/>.
    /// </summary>
    /// <typeparam name="TQuery">
    /// The type of query this handler processes. Must implement <see cref="IMediusQuery{TQueryResult}"/>.
    /// </typeparam>
    /// <typeparam name="TQueryResult">
    /// The type of result produced by the query.
    /// </typeparam>
    public abstract class MediusQueryHandler<TQuery, TQueryResult> : IMediusHandler, IMediusBaseHandler<TQuery, TQueryResult>
        where TQuery : IMediusQuery<TQueryResult>
    {
        /// <summary>
        /// Executes the query-specific logic and returns the resulting data.
        /// Override this method to implement the core behavior for the given query.
        /// </summary>
        /// <param name="query">The query instance to handle.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        /// A task containing the query result of type <typeparamref name="TQueryResult"/>.
        /// </returns>
        public abstract Task<TQueryResult?> HandleQueryAsync(TQuery query, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes <see cref="HandleQueryAsync"/>. 
        /// Provides a unified entry point for Medius to execute the handler.
        /// </summary>
        /// <param name="query">The query instance to handle.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        /// A task containing the query result of type <typeparamref name="TQueryResult"/>.
        /// </returns>
        public Task<TQueryResult?> HandleAsync(TQuery query, CancellationToken cancellationToken)
            => HandleQueryAsync(query, cancellationToken);
    }
}