using CompanyName.Api.Extensions;
using CompanyName.Application.Features.Uam.UpdateUserStatus;
using CompanyName.Domain.Enums;
using Mediator;

namespace CompanyName.Api.Endpoints.Uam;

internal sealed class UpdateUserStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        UamConstant.MapGroup(app)
            .MapPatch("/users/{id:int}/status", async (
            int id,
            UpdateUserStatusRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new UpdateUserStatusCommand(id, request.Status, request.RowVersion),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("UpdateUserStatus")
        .WithSummary("Update user status");
    }

    private sealed record UpdateUserStatusRequest(UserStatus Status, byte[] RowVersion);
}
