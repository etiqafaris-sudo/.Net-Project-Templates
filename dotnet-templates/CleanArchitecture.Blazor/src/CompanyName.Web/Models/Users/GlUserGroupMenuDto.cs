namespace CompanyName.Web.Models.Users
{
    public sealed class GlUserGroupMenuDto
    {
        public int Id { get; set; }

        public int? GroupId { get; set; }

        public int? MenuId { get; set; }

        public int IsActive { get; set; }
    }
}
