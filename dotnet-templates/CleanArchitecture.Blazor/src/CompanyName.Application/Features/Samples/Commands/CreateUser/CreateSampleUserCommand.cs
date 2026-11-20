using System.Data.Common;
using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities.Testings;
using CompanyName.Domain.Enums;
using Dapper;
using Mediator;

namespace CompanyName.Application.Features.Samples.Commands.CreateUser
{
    public sealed record CreateSampleUserCommand : IRequest<Unit>
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
    }

    internal sealed class CreateSampleUserCommandHandler : IRequestHandler<CreateSampleUserCommand>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public CreateSampleUserCommandHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async ValueTask<Unit> Handle(CreateSampleUserCommand request, CancellationToken cancellationToken)
        {
            using var con = _dbConnectionFactory.CreateConnection("TemplateDatabase");
            if (con is DbConnection dbConnection)
                await dbConnection.OpenAsync(cancellationToken);
            else
                con.Open();

            using var trans = con.BeginTransaction();

            var model = new TblUser
            {
                Email = request.Email,
                Name = request.Name,
            };
            model.SetAuditable(OpTypeEnum.Create);

            const string sql = """
                INSERT INTO Tbl_User (Name, Email, Aud_Add_Date, Aud_Add_By, Aud_Action)
                VALUES (@Name, @Email, @AudAddDate, @AudAddBy, @AudAction)
                """;

            await con.ExecuteAsync(new CommandDefinition(
                sql,
                model,
                transaction: trans,
                cancellationToken: cancellationToken));

            trans.Commit();
            return Unit.Value;
        }
    }
}
