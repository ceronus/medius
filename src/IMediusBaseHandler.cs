using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public interface IMediusBaseHandler<TOperation, TOperationResult>
    {
        Task<TOperationResult> HandleAsync(TOperation operation, CancellationToken cancellationToken);
    }
}