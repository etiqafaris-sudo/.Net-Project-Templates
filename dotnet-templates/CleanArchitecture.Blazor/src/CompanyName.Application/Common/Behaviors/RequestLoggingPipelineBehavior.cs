using System.Diagnostics;
using CompanyName.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Common.Behaviors;

internal sealed class RequestLoggingPipelineBehavior<TMessage, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : class, IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TMessage).Name;

        logger.LogInformation("Processing request {RequestName}", requestName);
        var stopwatch = Stopwatch.StartNew();
        var response = await next(message, cancellationToken);
        stopwatch.Stop();

        if (response is Result result)
        {
            if (result.IsFailure)
            {
                using (logger.BeginScope(new Dictionary<string, object> { ["Errors"] = result.Errors ?? [] }))
                {
                    logger.LogError("Completed request {RequestName} with error took {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                logger.LogInformation("Completed request {RequestName} took {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }

        logger.LogInformation("Completed request {RequestName} took {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
        return response;
    }
}
