using System.Net.Http;
using CompanyName.Api.Infrastructure.WebApi;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyName.Api.Infrastructure.Handlers;

internal sealed class RequestScopeHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var scope = Infrastructure.CompositionRoot.ServiceProvider.CreateScope();
        request.Properties[RequestScopeKeys.Scope] = scope;
        request.RegisterForDispose(scope);

        return await base.SendAsync(request, cancellationToken);
    }
}
