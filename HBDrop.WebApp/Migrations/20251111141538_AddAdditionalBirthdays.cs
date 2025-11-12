using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HBDrop.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalBirthdays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalBirthdays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthMonth = table.Column<int>(type: "integer", nullable: false),
                    BirthDay = table.Column<int>(type: "integer", nullable: false),
                    BirthYear = table.Column<int>(type: "integer", nullable: true),
                    Relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SendTo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SendToGroupId = table.Column<int>(type: "integer", nullable: true),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MessageHour = table.Column<int>(type: "integer", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalBirthdays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalBirthdays_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdditionalBirthdays_Contacts_SendToGroupId",
                        column: x => x.SendToGroupId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalBirthdays_BirthMonth_BirthDay",
                table: "AdditionalBirthdays",
                columns: new[] { "BirthMonth", "BirthDay" });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalBirthdays_ContactId",
                table: "AdditionalBirthdays",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalBirthdays_ContactId_IsEnabled",
                table: "AdditionalBirthdays",
                columns: new[] { "ContactId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalBirthdays_SendToGroupId",
                table: "AdditionalBirthdays",
                column: "SendToGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalBirthdays");
        }
    }
}
