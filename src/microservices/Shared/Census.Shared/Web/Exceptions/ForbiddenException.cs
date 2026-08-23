using Microsoft.AspNetCore.Http;

namespace Census.Shared.Web.Exceptions;

public class ForbiddenException : CensusException
{
    public ForbiddenException(string message = "Você não tem permissão para executar esta operação.")
        : base(message, "https://censo.local/errors/forbidden", StatusCodes.Status403Forbidden)
    {
    }
}
