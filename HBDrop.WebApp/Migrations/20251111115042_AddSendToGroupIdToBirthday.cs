using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBDrop.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSendToGroupIdToBirthday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendToGroupId",
                table: "Birthdays",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendToGroupId",
                table: "Birthdays");
        }
    }
}
