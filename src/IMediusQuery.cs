namespace Medius
{
    /// <summary>
    /// Represents a Medius operation that retrieves or computes data without modifying state.
    /// Queries are used for read-only operations and return a result of type <typeparamref name="TQueryResult"/>.
    /// </summary>
    /// <typeparam name="TQueryResult">
    /// The type of result returned by the query.
    /// </typeparam>
    public interface IMediusQuery<TQueryResult> : IMediusOperation<TQueryResult> { }
}