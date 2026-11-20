using CompanyName.Application.Common.Interfaces;
using Dapper;
using Mediator;

namespace CompanyName.Application.Features.Samples.Queries.DbfactoryQuerySample
{
    public sealed record DbFactorySampleQuery : IRequest<IReadOnlyList<DbFactorySampleQueryResponse>>;

    internal sealed class DbFactorySampleQueryHandler : IRequestHandler<DbFactorySampleQuery, IReadOnlyList<DbFactorySampleQueryResponse>>
    {
        private readonly IDbConnectionFactory _dbConnection;

        public DbFactorySampleQueryHandler(IDbConnectionFactory dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async ValueTask<IReadOnlyList<DbFactorySampleQueryResponse>> Handle(DbFactorySampleQuery request, CancellationToken cancellationToken)
        {
            using var con = _dbConnection.CreateConnection("TemplateDatabase");
            var result = (await con.QueryAsync<DbFactorySampleQueryResponse>("select User_id, FullName, Email from gl_users with (nolock) where is_deleted = 0 ",
                                                      commandType: System.Data.CommandType.Text)).AsList();
            if (result is null)
                return Array.Empty<DbFactorySampleQueryResponse>();

            return result;
        }
    }
}
