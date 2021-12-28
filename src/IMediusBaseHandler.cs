using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public interface IMediusBaseHandler<TOperation, TOperationResult>
    {
        /// <summary>
        /// Handle the injected operation (Action, Command or Query).
        /// </summary>
        /// <param name="operation">The injected Action, Command or Query.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns></returns>
        Task<TOperationResult> HandleAsync(TOperation operation, CancellationToken cancellationToken);
    }
}