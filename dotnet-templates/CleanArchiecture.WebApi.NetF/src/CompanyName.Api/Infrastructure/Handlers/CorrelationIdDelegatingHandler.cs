using System.Net.Http;
using Serilog.Context;

namespace CompanyName.Api.Infrastructure.Handlers;

internal sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId(request);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", request.GetHashCode().ToString()))
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.Headers.Contains(CorrelationIdHeaderName))
            {
                response.Headers.Add(CorrelationIdHeaderName, correlationId);
            }

            return response;
        }
    }

    private static string GetCorrelationId(HttpRequestMessage request)
    {
        if (request.Headers.TryGetValues(CorrelationIdHeaderName, out var values))
        {
            var value = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
