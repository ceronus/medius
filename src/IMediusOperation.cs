namespace Medius
{
    /// <summary>
    /// Defines the base contract for all Medius operations.
    /// An operation represents a unit of work that can be executed by a corresponding handler
    /// and may optionally produce a result of type <typeparamref name="TOperationResult"/>.
    /// </summary>
    /// <typeparam name="TOperationResult">
    /// The type of result produced by the operation.
    /// Use <see cref="MediusUndefinedType"/> when no result is expected.
    /// </typeparam>
    public interface IMediusOperation<TOperationResult> { }
}