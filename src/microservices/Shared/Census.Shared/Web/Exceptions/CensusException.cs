namespace Census.Shared.Web.Exceptions;

public abstract class CensusException : Exception
{
    protected CensusException(string message, string errorType, int statusCode)
        : base(message)
    {
        ErrorType = errorType;
        StatusCode = statusCode;
    }

    public string ErrorType { get; }
    public int StatusCode { get; }
}
