using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBDrop.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Contacts");
        }
    }
}
