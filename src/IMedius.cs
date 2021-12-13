using System.Threading;
using System.Threading.Tasks;

namespace Medius
{
    public interface IMedius
    {
        Task InvokeAsync(IMediusCommand command, CancellationToken cancellationToken = default);
        Task<TQueryResult> InvokeAsync<TQueryResult>(IMediusQuery<TQueryResult> query, CancellationToken cancellationToken = default);
        Task<TActionResult> InvokeAsync<TActionResult>(IMediusAction<TActionResult> action, CancellationToken cancellationToken = default);
    }
}