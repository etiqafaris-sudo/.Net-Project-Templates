namespace CompanyName.Web.Models.Users
{
    public sealed record GlUserGroupDto
    {
        public int Id { get; set; }

        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int IsActive { get; set; }
    }
}
