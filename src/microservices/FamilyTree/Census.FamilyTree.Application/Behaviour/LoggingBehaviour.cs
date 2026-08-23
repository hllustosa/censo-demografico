using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Census.FamilyTree.Application.Behaviour
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var name = typeof(TRequest).Name;
            _logger.LogInformation("Census FamilyTree Request: {Name} {@Request}", name, JsonConvert.SerializeObject(request));
            return await next();
        }
    }
}
