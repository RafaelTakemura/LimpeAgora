namespace LimpeAgoraApi.Exceptions;

public class DomainException : Exception
{
    public int StatusCode { get; }

    public DomainException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }
}

public class BusinessConflictException : DomainException
{
    public BusinessConflictException(string message) : base(message, 409)
    {
    }
}
