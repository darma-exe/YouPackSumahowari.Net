using System.Runtime.Serialization;

namespace YouPackSumahowari.Net.Exceptions;

[Serializable]
public class LoginRequiredException : Exception
{
    public LoginRequiredException()
        : base()
    {
    }

    public LoginRequiredException(string message)
        : base(message)
    {
    }

    public LoginRequiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected LoginRequiredException(SerializationInfo info, StreamingContext context)
    {
    }
}
