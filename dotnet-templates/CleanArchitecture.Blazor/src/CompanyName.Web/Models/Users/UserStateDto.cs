namespace CompanyName.Web.Models.Users
{
    public sealed record UserStateDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public string? IcNo { get; set; }
        public string? Role { get; set; }
    }
}
