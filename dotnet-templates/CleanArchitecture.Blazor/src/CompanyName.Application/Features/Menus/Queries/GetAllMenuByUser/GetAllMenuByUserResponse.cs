namespace CompanyName.Application.Features.Menus.Queries.GetAllMenuByUser
{
    public sealed record GetAllMenuByUserResponse
    {
        public int Id { get; set; }

        public string? MenuName { get; set; }

        public string? MenuUrl { get; set; }

        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int? OrderNo { get; set; }
    }
}
