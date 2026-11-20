namespace CompanyName.Api.Endpoints.Uam;

public static class UamConstant
{
    public static RouteGroupBuilder MapGroup(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/uam")
            .WithTags("UAM");
        return group;
    }
}
