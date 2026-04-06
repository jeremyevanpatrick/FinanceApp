using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp2.Api.Migrations.LoggingDb
{
    /// <inheritdoc />
    public partial class LogMessageMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MessageTemplate",
                table: "ApplicationLogs",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ApplicationLogs",
                type: "NVARCHAR(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MessageTemplate",
                table: "ApplicationLogs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ApplicationLogs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)");
        }
    }
}
