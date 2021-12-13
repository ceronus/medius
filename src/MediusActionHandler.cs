#pragma warning disable CA1725 // Parameter names should match base declaration
using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public abstract class MediusActionHandler<TAction, TActionResult> : IMediusHandler, IMediusBaseHandler<TAction, TActionResult>
        where TAction : IMediusAction<TActionResult>
    {
        public abstract Task<TActionResult> HandleActionAsync(TAction action, CancellationToken cancellationToken);

        public Task<TActionResult> HandleAsync(TAction action, CancellationToken cancellationToken)
            => HandleActionAsync(action, cancellationToken);
    }
}