using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public abstract class MediusQueryHandler<TQuery, TQueryResult> : IMediusHandler, IMediusBaseHandler<TQuery, TQueryResult>
        where TQuery : IMediusQuery<TQueryResult>
    {
        public abstract Task<TQueryResult?> HandleQueryAsync(TQuery query, CancellationToken cancellationToken);

        public Task<TQueryResult?> HandleAsync(TQuery query, CancellationToken cancellationToken)
            => HandleQueryAsync(query, cancellationToken);
    }
}