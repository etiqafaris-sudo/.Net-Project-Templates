namespace CompanyName.Application.Common.Models
{
    public sealed record GlParameterResponse(
        int GlParameterId,
        string ParameterId,
        string? ParameterCode,
        int ParameterTypeId,
        string? ParameterDescription,
        string? ParameterDescriptionEn,
        int? Sort);
}
