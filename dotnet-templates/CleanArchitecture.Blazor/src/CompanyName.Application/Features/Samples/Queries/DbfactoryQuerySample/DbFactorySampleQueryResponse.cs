namespace CompanyName.Application.Features.Samples.Queries.DbfactoryQuerySample
{
    public sealed record DbFactorySampleQueryResponse
    {
        public int user_id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
