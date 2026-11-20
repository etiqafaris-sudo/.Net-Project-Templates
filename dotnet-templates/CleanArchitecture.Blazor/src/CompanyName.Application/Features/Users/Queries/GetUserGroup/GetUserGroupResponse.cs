namespace CompanyName.Application.Features.Users.Queries.GetUserGroup
{
    public sealed record GetUserGroupResponse(int Id, string? GroupCode, string? GroupName, int IsActive);
}
