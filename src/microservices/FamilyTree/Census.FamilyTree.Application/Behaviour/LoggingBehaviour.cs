using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Census.FamilyTree.Application.Behaviour
{
    public class LoggingBehaviour
    {
        public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
        {
            private readonly ILogger<LoggingBehavior<TRequest, TResponse>> Logger;

            public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
            {
                Logger = logger;
            }

            public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
            {
                var name = typeof(TRequest).Name;
                Logger.LogInformation("Census People Request: {Name} {@Request}", name, JsonConvert.SerializeObject(request));
                return next();
            }

        }
    }
}
