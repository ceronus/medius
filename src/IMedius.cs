using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public interface IMedius
    {
        /// <summary>
        /// Invoke the associated <see cref="MediusCommandHandler{IMediusCommand}"/>.
        /// </summary>
        /// <param name="command">The <see cref="IMediusCommand"/> that will be injected to the handler.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>The associated handler's <see cref="Task"/>.</returns>
        Task InvokeAsync(IMediusCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the associated <see cref="MediusQueryHandler{TQuery, IMediusQuery{TQueryResult}}"/>.
        /// </summary>
        /// <param name="query">The <see cref="IMediusQuery{TQueryResult}"/> that will be injected to the associated handler.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>The associated handler's <see cref="Task"/>.</returns>
        Task<TQueryResult?> InvokeAsync<TQueryResult>(IMediusQuery<TQueryResult> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the associated <see cref="MediusActionHandler{IMediusAction{TActionResult}, TActionResult}"/>.
        /// </summary>
        /// <param name="action">The <see cref="IMediusAction{TActionResult}"/> that will be injected to the associated handler.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>The associated handler's <see cref="Task"/>.</returns>
        Task<TActionResult?> InvokeAsync<TActionResult>(IMediusAction<TActionResult> action, CancellationToken cancellationToken = default);
    }
}