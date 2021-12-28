using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public abstract class MediusCommandHandler<TCommand> : IMediusHandler, IMediusBaseHandler<TCommand, MediusUndefinedType>
        where TCommand : IMediusCommand
    {
        public abstract Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

        public async Task<MediusUndefinedType> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await HandleCommandAsync(command, cancellationToken).ConfigureAwait(false);
            return default;
        }
    }
}