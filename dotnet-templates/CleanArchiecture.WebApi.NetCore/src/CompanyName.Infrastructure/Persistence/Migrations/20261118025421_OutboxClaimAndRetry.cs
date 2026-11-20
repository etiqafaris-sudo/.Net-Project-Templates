using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyName.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OutboxClaimAndRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_unprocessed",
                table: "outbox_messages");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "outbox_messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntilUtc",
                table: "outbox_messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_unprocessed",
                table: "outbox_messages",
                column: "OccurredOnUtc",
                filter: "[ProcessedOnUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_unprocessed",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_unprocessed",
                table: "outbox_messages",
                columns: new[] { "OccurredOnUtc", "ProcessedOnUtc" },
                filter: "[ProcessedOnUtc] IS NULL");
        }
    }
}
