using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace CompanyName.Api.Extensions;

internal static class Results
{
    public static IHttpActionResult Ok(object? value = null) =>
        new ResultActionResult(HttpStatusCode.OK, value);

    public static IHttpActionResult NoContent() =>
        new StatusCodeActionResult(HttpStatusCode.NoContent);

    public static IHttpActionResult Created(string location, object value) =>
        new CreatedActionResult(location, value);

    public static IHttpActionResult Problem(string? title, string? detail, int statusCode) =>
        new ProblemActionResult(title, detail, statusCode);
}

internal sealed class ResultActionResult : IHttpActionResult
{
    private readonly HttpStatusCode _statusCode;
    private readonly object? _value;

    public ResultActionResult(HttpStatusCode statusCode, object? value)
    {
        _statusCode = statusCode;
        _value = value;
    }

    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = _value is null ? null : new ObjectContent(_value.GetType(), _value, new JsonMediaTypeFormatter()),
        };
        return Task.FromResult(response);
    }
}

internal sealed class StatusCodeActionResult : IHttpActionResult
{
    private readonly HttpStatusCode _statusCode;

    public StatusCodeActionResult(HttpStatusCode statusCode) => _statusCode = statusCode;

    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(_statusCode));
}

internal sealed class CreatedActionResult : IHttpActionResult
{
    private readonly string _location;
    private readonly object _value;

    public CreatedActionResult(string location, object value)
    {
        _location = location;
        _value = value;
    }

    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new ObjectContent(_value.GetType(), _value, new JsonMediaTypeFormatter()),
        };
        response.Headers.Location = new System.Uri(_location, UriKind.RelativeOrAbsolute);
        return Task.FromResult(response);
    }
}

internal sealed class ProblemActionResult : IHttpActionResult
{
    private readonly string? _title;
    private readonly string? _detail;
    private readonly int _statusCode;

    public ProblemActionResult(string? title, string? detail, int statusCode)
    {
        _title = title;
        _detail = detail;
        _statusCode = statusCode;
    }

    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
        var body = new { title = _title, detail = _detail, status = _statusCode };
        var response = new HttpResponseMessage((HttpStatusCode)_statusCode)
        {
            Content = new ObjectContent(body.GetType(), body, new JsonMediaTypeFormatter()),
        };
        return Task.FromResult(response);
    }
}
