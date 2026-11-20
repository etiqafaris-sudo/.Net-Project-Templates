using CompanyName.Api.Extensions;
using CompanyName.Application.Features.Uam.CreateStaff;
using Mediator;

namespace CompanyName.Api.Endpoints.Uam;

internal sealed class CreateStaffEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        UamConstant.MapGroup(app)
            .MapPost("/staff", async (
            CreateStaffRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new CreateStaffCommand(request.Email, request.FullName),
                cancellationToken);

            return result.ToHttpResult(id => Results.Created($"/api/v1/uam/users/{id}", new { id }));
        })
        .WithName("CreateStaff")
        .WithSummary("Create a staff user");
    }

    private sealed record CreateStaffRequest(string Email, string FullName);
}
