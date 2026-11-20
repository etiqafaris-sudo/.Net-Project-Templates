using System.Web.Http;
using CompanyName.Api.Extensions;
using CompanyName.Application.Features.Uam.CreateStaff;
using CompanyName.Application.Features.Uam.ListUsers;
using CompanyName.Application.Features.Uam.UpdateUserStatus;
using CompanyName.Domain.Enums;
using Mediator;

namespace CompanyName.Api.Controllers.Uam;

[RoutePrefix(UamConstant.RoutePrefix)]
public sealed class UamController : ApiController
{
    private readonly IMediator _mediator;

    public UamController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("users")]
    [HttpGet]
    public async Task<IHttpActionResult> ListUsers(
        [FromUri] int page = 1,
        [FromUri] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new ListUsersQuery(page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResult(data => Results.Ok(data));
    }

    [Route("staff")]
    [HttpPost]
    public async Task<IHttpActionResult> CreateStaff(
        CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateStaffCommand(request.Email, request.FullName),
            cancellationToken);

        return result.ToHttpResult(id => Results.Created($"/api/v1/uam/users/{id}", new { id }));
    }

    [Route("users/{id:int}/status")]
    [HttpPatch]
    public async Task<IHttpActionResult> UpdateStatus(
        int id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateUserStatusCommand(id, request.Status, request.RowVersion),
            cancellationToken);

        return result.ToHttpResult();
    }

    public sealed class CreateStaffRequest
    {
        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
    }

    public sealed class UpdateUserStatusRequest
    {
        public UserStatus Status { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
