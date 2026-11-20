namespace CompanyName.Web.Models.Settings
{
    public sealed class GlParameterDto
    {
        public int GlParameterId { get; set; }

        public string ParameterId { get; set; } = string.Empty;

        public string? ParameterCode { get; set; }

        public int ParameterTypeId { get; set; }

        public string? ParameterDescription { get; set; }

        public string? ParameterDescriptionEn { get; set; }

        public int? Sort { get; set; }
    }
}
