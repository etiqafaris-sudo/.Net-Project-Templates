using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyName.Api.Infrastructure.WebApi;

internal sealed class ServiceProviderHttpControllerActivator : IHttpControllerActivator
{
    private readonly IServiceProvider _provider;

    public ServiceProviderHttpControllerActivator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IHttpController Create(
        HttpRequestMessage request,
        HttpControllerDescriptor controllerDescriptor,
        Type controllerType)
    {
        var scope = RequestScopeKeys.GetScope(request);
        return (IHttpController)scope.ServiceProvider.GetRequiredService(controllerType);
    }
}