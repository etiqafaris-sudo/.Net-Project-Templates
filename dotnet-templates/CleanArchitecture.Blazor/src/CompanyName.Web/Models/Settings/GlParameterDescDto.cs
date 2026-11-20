namespace CompanyName.Web.Models.Settings
{
    public sealed class GlParameterDescDto
    {
        public int ParameterTypeId { get; set; }
        public string? ParameterId { get; set; }
        public string? ParameterDescription { get; set; }
        public int? Sort { get; set; }
        public int IsActive { get; set; }
    }
}
