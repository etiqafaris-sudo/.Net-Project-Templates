using System.Web.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CompanyName.Api.Controllers;

[RoutePrefix("health")]
public sealed class HealthController : ApiController
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [Route("")]
    [HttpGet]
    public async Task<IHttpActionResult> GetAll(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        return BuildResult(report);
    }

    [Route("live")]
    [HttpGet]
    public IHttpActionResult Live() =>
        Ok(new { status = "Healthy" });

    [Route("ready")]
    [HttpGet]
    public async Task<IHttpActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            predicate: check => check.Tags.Contains("ready") || check.Name == "database",
            cancellationToken: cancellationToken);
        return BuildResult(report);
    }

    private IHttpActionResult BuildResult(HealthReport report)
    {
        var statusCode = report.Status == HealthStatus.Healthy ? 200 :
            report.Status == HealthStatus.Degraded ? 200 : 503;

        var body = new
        {
            status = report.Status.ToString(),
            entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                }),
        };

        return Content((System.Net.HttpStatusCode)statusCode, body);
    }
}
