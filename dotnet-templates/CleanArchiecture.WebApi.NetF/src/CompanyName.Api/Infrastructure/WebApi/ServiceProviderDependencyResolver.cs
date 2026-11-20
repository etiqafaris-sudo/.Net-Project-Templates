using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyName.Api.Infrastructure.WebApi;

internal sealed class ServiceProviderDependencyResolver : IDependencyResolver
{
    private readonly IServiceProvider _provider;

    public ServiceProviderDependencyResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public object? GetService(Type serviceType) => _provider.GetService(serviceType);

    public IEnumerable<object?> GetServices(Type serviceType) => _provider.GetServices(serviceType);

    public IDependencyScope BeginScope()
    {
        return new ServiceProviderDependencyScope(_provider.CreateScope());
    }

    public void Dispose()
    {
    }
}

internal sealed class ServiceProviderDependencyScope : IDependencyScope
{
    private readonly IServiceScope _scope;

    public ServiceProviderDependencyScope(IServiceScope scope)
    {
        _scope = scope;
    }

    public object? GetService(Type serviceType) => _scope.ServiceProvider.GetService(serviceType);

    public IEnumerable<object?> GetServices(Type serviceType) =>
        _scope.ServiceProvider.GetServices(serviceType);

    public void Dispose() => _scope.Dispose();
}