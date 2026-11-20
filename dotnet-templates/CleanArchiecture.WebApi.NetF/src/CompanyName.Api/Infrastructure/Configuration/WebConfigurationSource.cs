using Microsoft.Extensions.Configuration;

namespace CompanyName.Api.Infrastructure.Configuration;

internal sealed class WebConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new WebConfigurationProvider();
}