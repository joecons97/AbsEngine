using System.Runtime.Serialization;

namespace AbsEngine.Exceptions;

public class GameInstanceException : Exception
{
    public GameInstanceException()
    {
    }

    public GameInstanceException(string? message) : base(message)
    {
    }

    public GameInstanceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GameInstanceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
