using Microsoft.Extensions.Configuration;

namespace CompanyName.Api.Infrastructure.Configuration;

internal static class WebConfigurationExtensions
{
    public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder) =>
        builder.Add(new WebConfigurationSource());
}