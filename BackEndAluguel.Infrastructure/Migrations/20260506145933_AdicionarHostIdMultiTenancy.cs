using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAluguel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHostIdMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Apartamentos_Numero_Bloco",
                table: "Apartamentos");

            migrationBuilder.DeleteData(
                table: "Configuracoes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "HostId",
                table: "Inquilinos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HostId",
                table: "Faturas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HostId",
                table: "Configuracoes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HostId",
                table: "Apartamentos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Configuracoes_HostId",
                table: "Configuracoes",
                column: "HostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Apartamentos_HostId_Numero_Bloco",
                table: "Apartamentos",
                columns: new[] { "HostId", "Numero", "Bloco" },
                unique: true,
                filter: "[Bloco] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Apartamentos_Hosts_HostId",
                table: "Apartamentos",
                column: "HostId",
                principalTable: "Hosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Configuracoes_Hosts_HostId",
                table: "Configuracoes",
                column: "HostId",
                principalTable: "Hosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartamentos_Hosts_HostId",
                table: "Apartamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Configuracoes_Hosts_HostId",
                table: "Configuracoes");

            migrationBuilder.DropIndex(
                name: "IX_Configuracoes_HostId",
                table: "Configuracoes");

            migrationBuilder.DropIndex(
                name: "IX_Apartamentos_HostId_Numero_Bloco",
                table: "Apartamentos");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "Inquilinos");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "Faturas");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "Apartamentos");

            migrationBuilder.InsertData(
                table: "Configuracoes",
                columns: new[] { "Id", "AtualizadoEm", "ChavePix", "CidadeRecebedorPix", "CriadoEm", "KwhValor", "MensagemPadraoWhatsapp", "NomeRecebedorPix", "NumeroWhatsappLocador", "ValorAgua", "WalletIdAsaas" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.0m, null, null, null, 0.0m, null });

            migrationBuilder.CreateIndex(
                name: "IX_Apartamentos_Numero_Bloco",
                table: "Apartamentos",
                columns: new[] { "Numero", "Bloco" },
                unique: true,
                filter: "[Bloco] IS NOT NULL");
        }
    }
}
