using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HBDrop.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GifUrl",
                table: "Birthdays",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GifUrl",
                table: "AdditionalBirthdays",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    EventName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventMonth = table.Column<int>(type: "integer", nullable: false),
                    EventDay = table.Column<int>(type: "integer", nullable: false),
                    EventYear = table.Column<int>(type: "integer", nullable: true),
                    CustomMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GifUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    IsRegionalEvent = table.Column<bool>(type: "boolean", nullable: false),
                    RegionalEventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegionalEventCountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MessageHour = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomEvents_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomEvents_ContactId",
                table: "CustomEvents",
                column: "ContactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomEvents");

            migrationBuilder.DropColumn(
                name: "GifUrl",
                table: "Birthdays");

            migrationBuilder.DropColumn(
                name: "GifUrl",
                table: "AdditionalBirthdays");
        }
    }
}
