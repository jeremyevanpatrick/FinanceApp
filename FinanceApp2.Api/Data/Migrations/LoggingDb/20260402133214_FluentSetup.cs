using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp2.Api.Migrations.LoggingDb
{
    /// <inheritdoc />
    public partial class FluentSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MessageTemplate = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Exception = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ApplicationLogs (Level, ApplicationName, ServerName, ErrorCode, Message, Exception, [Timestamp])
                SELECT 'Error', ApplicationName, 'DESKTOP-GQ0O2U8', LEFT(ErrorCode, 32), ErrorMessage, Notes, [Timestamp]
                FROM Errors
            ");

            migrationBuilder.DropTable(
                name: "Errors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Errors",
                columns: table => new
                {
                    ErrorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.ErrorId);
                });

            migrationBuilder.Sql(@"
                INSERT INTO Errors (ApplicationName, ErrorCode, ErrorMessage, Notes, [Timestamp])
                SELECT ApplicationName, ErrorCode, Message, Exception, [Timestamp]
                FROM ApplicationLogs
            ");

            migrationBuilder.DropTable(
                name: "ApplicationLogs");
        }
    }
}
