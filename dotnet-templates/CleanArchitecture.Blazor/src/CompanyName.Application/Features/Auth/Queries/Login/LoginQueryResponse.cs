namespace CompanyName.Application.Features.Auth.Queries.Login
{
    public sealed record LoginQueryResponse
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string? IcNo { get; set; }
        public string? Role { get; set; }
    }
}
