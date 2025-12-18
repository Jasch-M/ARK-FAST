using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArkaZilla.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "arkad");

            migrationBuilder.CreateTable(
                name: "Loggings",
                schema: "arkad",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CallName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loggings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "arkad",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IdentityAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    HasConsented = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultLoggingsForCategories",
                schema: "arkad",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LoggingId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultLoggingsForCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultLoggingsForCategories_Loggings_LoggingId",
                        column: x => x.LoggingId,
                        principalSchema: "arkad",
                        principalTable: "Loggings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DefaultLoggingsForServers",
                schema: "arkad",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LoggingId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultLoggingsForServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultLoggingsForServers_Loggings_LoggingId",
                        column: x => x.LoggingId,
                        principalSchema: "arkad",
                        principalTable: "Loggings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogLocations",
                schema: "arkad",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IdentifierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LoggingId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogLocations_Loggings_LoggingId",
                        column: x => x.LoggingId,
                        principalSchema: "arkad",
                        principalTable: "Loggings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultLoggingsForCategories_LoggingId",
                schema: "arkad",
                table: "DefaultLoggingsForCategories",
                column: "LoggingId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultLoggingsForServers_LoggingId",
                schema: "arkad",
                table: "DefaultLoggingsForServers",
                column: "LoggingId");

            migrationBuilder.CreateIndex(
                name: "IX_LogLocations_LoggingId",
                schema: "arkad",
                table: "LogLocations",
                column: "LoggingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultLoggingsForCategories",
                schema: "arkad");

            migrationBuilder.DropTable(
                name: "DefaultLoggingsForServers",
                schema: "arkad");

            migrationBuilder.DropTable(
                name: "LogLocations",
                schema: "arkad");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "arkad");

            migrationBuilder.DropTable(
                name: "Loggings",
                schema: "arkad");
        }
    }
}
