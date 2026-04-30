using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAluguel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarWalletIdAsaasNaConfiguracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalletIdAsaas",
                table: "Configuracoes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Configuracoes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "WalletIdAsaas",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletIdAsaas",
                table: "Configuracoes");
        }
    }
}
