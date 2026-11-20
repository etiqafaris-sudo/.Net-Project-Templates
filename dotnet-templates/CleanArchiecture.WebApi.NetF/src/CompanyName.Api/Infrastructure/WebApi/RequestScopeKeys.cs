using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyName.Api.Infrastructure.WebApi;

internal static class RequestScopeKeys
{
    public const string Scope = "RequestServiceScope";

    public static IServiceScope GetScope(HttpRequestMessage request)
    {
        if (request.Properties.TryGetValue(Scope, out var scopeObject) && scopeObject is IServiceScope scope)
        {
            return scope;
        }

        throw new InvalidOperationException("Request scope is not available.");
    }
}