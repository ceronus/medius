namespace Medius
{
    /// <summary>
    /// Represents a Medius operation that performs an action and produces a result.
    /// Actions are typically used for operations that modify state or trigger behavior 
    /// and return a value of type <typeparamref name="TActionResult"/>.
    /// </summary>
    /// <typeparam name="TActionResult">
    /// The type of result returned by the action.
    /// </typeparam>
    public interface IMediusAction<TActionResult> : IMediusOperation<TActionResult> { }
}