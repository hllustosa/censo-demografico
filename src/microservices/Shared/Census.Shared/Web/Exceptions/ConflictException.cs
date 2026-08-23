using Microsoft.AspNetCore.Http;

namespace Census.Shared.Web.Exceptions;

public class ConflictException : CensusException
{
    public ConflictException(string message)
        : base(message, "https://censo.local/errors/conflict", StatusCodes.Status409Conflict)
    {
    }
}
