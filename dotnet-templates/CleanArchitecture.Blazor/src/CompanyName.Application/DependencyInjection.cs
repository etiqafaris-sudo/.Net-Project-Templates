using CompanyName.Application.Common.Behaviors;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CompanyName.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(RequestLoggingPipelineBehavior<,>)];
        });

        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 10_000; // Limit total cache size to prevent unbounded growth
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<ICookieAuthService, CookieAuthService>();
        services.AddScoped<IMenuAccessService, MenuAccessService>();
        services.AddScoped<ILookupParameterService, LookupParameterService>();
        services.AddScoped<IAttachmentFileService, AttachmentFileService>();
        return services;
    }
}
