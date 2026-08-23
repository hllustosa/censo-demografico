using Microsoft.AspNetCore.Http;

namespace Census.Shared.Web.Exceptions;

public class NotFoundException : CensusException
{
    public NotFoundException(string message)
        : base(message, "https://censo.local/errors/not-found", StatusCodes.Status404NotFound)
    {
    }
}
