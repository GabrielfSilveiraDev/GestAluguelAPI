using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAluguel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarValorGaragemEWhatsAppPix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorGaragem",
                table: "Faturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ChavePix",
                table: "Configuracoes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CidadeRecebedorPix",
                table: "Configuracoes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensagemPadraoWhatsapp",
                table: "Configuracoes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeRecebedorPix",
                table: "Configuracoes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroWhatsappLocador",
                table: "Configuracoes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Configuracoes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "ChavePix", "CidadeRecebedorPix", "MensagemPadraoWhatsapp", "NomeRecebedorPix", "NumeroWhatsappLocador" },
                values: new object[] { null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorGaragem",
                table: "Faturas");

            migrationBuilder.DropColumn(
                name: "ChavePix",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "CidadeRecebedorPix",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "MensagemPadraoWhatsapp",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "NomeRecebedorPix",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "NumeroWhatsappLocador",
                table: "Configuracoes");
        }
    }
}
