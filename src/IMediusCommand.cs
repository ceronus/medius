namespace Medius
{
    /// <summary>
    /// Represents a Medius operation that performs an action but does not return a result.
    /// Commands are executed for their side effects and use <see cref="MediusUndefinedType"/> as their result type.
    /// </summary>
    public interface IMediusCommand : IMediusOperation<MediusUndefinedType> { }
}