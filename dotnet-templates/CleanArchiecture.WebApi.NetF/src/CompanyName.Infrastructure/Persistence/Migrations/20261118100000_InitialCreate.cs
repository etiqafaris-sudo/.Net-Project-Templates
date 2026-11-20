namespace CompanyName.Infrastructure.Persistence.Migrations;

using System.Data.Entity.Migrations;

public partial class InitialCreate : DbMigration
{
    public override void Up()
    {
        CreateTable(
            "dbo.users",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                Email = c.String(nullable: false, maxLength: 256),
                FullName = c.String(nullable: false, maxLength: 256),
                Status = c.Int(nullable: false),
                CreatedAtUtc = c.DateTime(nullable: false, precision: 7),
                ModifiedAtUtc = c.DateTime(precision: 7),
                RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
            })
            .PrimaryKey(t => t.Id);

        CreateIndex("dbo.users", "Email", unique: true, name: "IX_users_Email");

        CreateTable(
            "dbo.outbox_messages",
            c => new
            {
                Id = c.Guid(nullable: false),
                Type = c.String(nullable: false, maxLength: 512),
                Content = c.String(nullable: false),
                OccurredOnUtc = c.DateTime(nullable: false, precision: 7),
                ProcessedOnUtc = c.DateTime(precision: 7),
                Error = c.String(),
                LockedUntilUtc = c.DateTime(precision: 7),
                AttemptCount = c.Int(nullable: false),
            })
            .PrimaryKey(t => t.Id);

        Sql("CREATE INDEX IX_outbox_messages_unprocessed ON dbo.outbox_messages(OccurredOnUtc) WHERE ProcessedOnUtc IS NULL");
    }

    public override void Down()
    {
        DropIndex("dbo.outbox_messages", "IX_outbox_messages_unprocessed");
        DropTable("dbo.outbox_messages");
        DropIndex("dbo.users", "IX_users_Email");
        DropTable("dbo.users");
    }
}