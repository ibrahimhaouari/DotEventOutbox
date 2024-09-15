using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotEventOutbox.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessing",
                schema: "outbox",
                table: "OutboxMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessing",
                schema: "outbox",
                table: "OutboxMessages");
        }
    }
}
