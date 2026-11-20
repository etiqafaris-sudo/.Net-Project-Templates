namespace CompanyName.Shared.Configurations
{
    public sealed class AppSettings
    {
        public required IReadOnlyList<DbConnections> DbConnections { get; set; }
        public string AppKey { get; set; }
        public string ApplicationName { get; set; }
    }

    public sealed class DbConnections
    {
        public string Db_Name { get; set; }
        public DatabaseProvider Provider { get; set; }
        public string ConnectionString { get; set; }
    }
}
