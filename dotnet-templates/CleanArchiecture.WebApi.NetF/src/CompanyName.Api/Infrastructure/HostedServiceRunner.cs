using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompanyName.Api.Infrastructure;

public sealed class HostedServiceRunner
{
    public static HostedServiceRunner Instance { get; private set; } = null!;

    private readonly IReadOnlyList<IHostedService> _services;
    private readonly ILogger<HostedServiceRunner> _logger;

    public static void Initialize(IServiceProvider provider)
    {
        Instance = new HostedServiceRunner(
            provider.GetServices<IHostedService>().ToList(),
            provider.GetRequiredService<ILogger<HostedServiceRunner>>());
    }

    private HostedServiceRunner(IReadOnlyList<IHostedService> services, ILogger<HostedServiceRunner> logger)
    {
        _services = services;
        _logger = logger;
    }

    public void Start()
    {
        foreach (var service in _services)
        {
            service.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            _logger.LogInformation("Started hosted service {HostedService}", service.GetType().Name);
        }
    }

    public void Stop()
    {
        for (var i = _services.Count - 1; i >= 0; i--)
        {
            try
            {
                _services[i].StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to stop hosted service {HostedService}", _services[i].GetType().Name);
            }
        }
    }
}
