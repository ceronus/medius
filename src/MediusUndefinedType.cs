namespace Medius
{
    /// <summary>
    /// Represents a placeholder type used when a Medius operation does not produce a result.
    /// This type is used as the generic argument for operations such as <see cref="IMediusCommand"/> 
    /// where a return value is not required.
    /// </summary>
    public sealed class MediusUndefinedType { }
}