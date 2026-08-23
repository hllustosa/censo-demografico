using Microsoft.AspNetCore.Http;

namespace Census.Shared.Web.Exceptions;

public class BusinessRuleException : CensusException
{
    public BusinessRuleException(string message)
        : base(message, "https://censo.local/errors/business-rule", StatusCodes.Status422UnprocessableEntity)
    {
    }
}
