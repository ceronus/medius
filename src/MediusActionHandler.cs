using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    /// <summary>
    /// Provides a base class for handling <see cref="IMediusAction{TActionResult}"/> operations.
    /// Action handlers execute logic that performs an operation and returns a result of type
    /// <typeparamref name="TActionResult"/>.
    /// </summary>
    /// <typeparam name="TAction">
    /// The type of action this handler processes. Must implement <see cref="IMediusAction{TActionResult}"/>.
    /// </typeparam>
    /// <typeparam name="TActionResult">
    /// The type of result produced by the action.
    /// </typeparam>
    public abstract class MediusActionHandler<TAction, TActionResult> : IMediusHandler, IMediusBaseHandler<TAction, TActionResult>
        where TAction : IMediusAction<TActionResult>
    {
        /// <summary>
        /// Executes the action-specific logic and returns a result.
        /// Override this method to implement the core behavior for the given action.
        /// </summary>
        /// <param name="action">The action instance to handle.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        /// A task containing the action result of type <typeparamref name="TActionResult"/>.
        /// </returns>
        public abstract Task<TActionResult?> HandleActionAsync(TAction action, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes <see cref="HandleActionAsync"/>. 
        /// Provides a unified entry point for Medius to execute the handler.
        /// </summary>
        /// <param name="action">The action instance to handle.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        /// A task containing the action result of type <typeparamref name="TActionResult"/>.
        /// </returns>
        public Task<TActionResult?> HandleAsync(TAction action, CancellationToken cancellationToken)
            => HandleActionAsync(action, cancellationToken);
    }
}