namespace CompanyName.Application.Common.Models
{
    public sealed record Gl_ParameterDescriptionResponse(int ParameterTypeId,
        string? ParameterId,
        string? ParameterDescription,
        int? Sort,int IsActive);
}
