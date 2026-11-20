using CompanyName.Api.Extensions;
using CompanyName.Application.Features.Uam.ListUsers;
using Mediator;

namespace CompanyName.Api.Endpoints.Uam;

internal sealed class ListUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        UamConstant.MapGroup(app).
               MapGet("/users", async (
               int? page,
               int? pageSize,
               IMediator mediator,
               CancellationToken cancellationToken) =>
           {
               var query = new ListUsersQuery(page ?? 1, pageSize ?? 50);
               var result = await mediator.Send(query, cancellationToken);
               return result.ToHttpResult(Results.Ok);
           })
           .WithName("ListUsers")
           .WithSummary("List users (paginated)");
    }
}
