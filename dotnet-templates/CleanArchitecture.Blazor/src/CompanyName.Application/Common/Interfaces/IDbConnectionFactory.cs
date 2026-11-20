using System.Data;
using CompanyName.Shared.Configurations;

namespace CompanyName.Application.Common.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection(string name);
    IDbConnection CreateConnection(DatabaseProvider provider, string connectionStringName);
}
