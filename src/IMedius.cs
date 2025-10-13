using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    /// <summary>
    /// Defines the Medius operation dispatcher interface. 
    /// Provides methods to execute <see cref="IMediusCommand"/>, <see cref="IMediusQuery{TQueryResult}"/>, 
    /// and <see cref="IMediusAction{TActionResult}"/> operations by routing them to their corresponding handlers.
    /// </summary>
    public interface IMedius
    {
        /// <summary>
        /// Executes the <see cref="IMediusCommand"/> by locating and invoking its associated 
        /// <see cref="MediusCommandHandler{TCommand}"/>.
        /// </summary>
        /// <param name="command">The command instance to execute.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InvokeAsync(IMediusCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the <see cref="IMediusQuery{TQueryResult}"/> by locating and invoking its associated 
        /// <see cref="MediusQueryHandler{TQuery, TQueryResult}"/>.
        /// </summary>
        /// <typeparam name="TQueryResult">The expected result type of the query.</typeparam>
        /// <param name="query">The query instance to execute.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task containing the query result.</returns>
        Task<TQueryResult?> InvokeAsync<TQueryResult>(IMediusQuery<TQueryResult> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the <see cref="IMediusAction{TActionResult}"/> by locating and invoking its associated 
        /// <see cref="MediusActionHandler{TAction, TActionResult}"/>.
        /// </summary>
        /// <typeparam name="TActionResult">The expected result type of the action.</typeparam>
        /// <param name="action">The action instance to execute.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task containing the action result.</returns>
        Task<TActionResult?> InvokeAsync<TActionResult>(IMediusAction<TActionResult> action, CancellationToken cancellationToken = default);
    }
}