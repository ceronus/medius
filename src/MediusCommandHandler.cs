using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    /// <summary>
    /// Provides a base class for handling <see cref="IMediusCommand"/> operations.
    /// Command handlers execute logic that performs an action but do not return a result.
    /// </summary>
    /// <typeparam name="TCommand">
    /// The type of command this handler processes. Must implement <see cref="IMediusCommand"/>.
    /// </typeparam>
    public abstract class MediusCommandHandler<TCommand> : IMediusHandler, IMediusBaseHandler<TCommand, MediusUndefinedType>
        where TCommand : IMediusCommand
    {
        /// <summary>
        /// Executes the command-specific logic.
        /// Override this method to implement the core behavior for the given command.
        /// </summary>
        /// <param name="command">The command instance to handle.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes <see cref="HandleCommandAsync"/> and returns <see langword="default"/> 
        /// to satisfy the generic <see cref="IMediusBaseHandler{TCommand, TOperationResult}"/> contract.
        /// </summary>
        /// <param name="command">The command instance to handle.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        /// A completed task containing <see langword="null"/>, since commands do not produce results.
        /// </returns>
        public async Task<MediusUndefinedType?> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await HandleCommandAsync(command, cancellationToken).ConfigureAwait(false);
            return default;
        }
    }
}