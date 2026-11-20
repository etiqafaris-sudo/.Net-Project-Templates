using System.Web.Http;
using CompanyName.Api.Infrastructure;
using CompanyName.Api.Infrastructure.Filters;
using CompanyName.Api.Infrastructure.Handlers;
using CompanyName.Api.Infrastructure.WebApi;

namespace CompanyName.Api;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        config.MapHttpAttributeRoutes();
        config.EnsureInitialized();

        config.DependencyResolver = new ServiceProviderDependencyResolver(CompositionRoot.ServiceProvider);
        config.Services.Replace(
            typeof(System.Web.Http.Dispatcher.IHttpControllerActivator),
            new ServiceProviderHttpControllerActivator(CompositionRoot.ServiceProvider));

        config.Filters.Add(new GlobalExceptionFilter());
        config.MessageHandlers.Add(new RequestScopeHandler());
        config.MessageHandlers.Add(new CorrelationIdDelegatingHandler());

        config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling =
            Newtonsoft.Json.NullValueHandling.Ignore;
    }
}