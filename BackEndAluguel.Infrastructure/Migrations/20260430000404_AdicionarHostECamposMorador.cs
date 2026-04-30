using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAluguel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHostECamposMorador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoCivil",
                table: "Inquilinos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrgaoEmissor",
                table: "Inquilinos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rg",
                table: "Inquilinos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Inquilinos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Hosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "nchar(11)", fixedLength: true, maxLength: 11, nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmailConfirmado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TokenConfirmacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenExpiracaoConfirmacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_Cpf",
                table: "Hosts",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_Email",
                table: "Hosts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_TokenConfirmacao",
                table: "Hosts",
                column: "TokenConfirmacao",
                unique: true,
                filter: "[TokenConfirmacao] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hosts");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Inquilinos");

            migrationBuilder.DropColumn(
                name: "OrgaoEmissor",
                table: "Inquilinos");

            migrationBuilder.DropColumn(
                name: "Rg",
                table: "Inquilinos");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Inquilinos");
        }
    }
}
