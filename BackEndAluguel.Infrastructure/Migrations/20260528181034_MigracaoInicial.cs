using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAluguel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NomeCompleto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 11, nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EmailConfirmado = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    TokenConfirmacao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TokenExpiracaoConfirmacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Apartamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Bloco = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true, defaultValue: ""),
                    Ocupado = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    HostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apartamentos_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KwhValor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    ValorAgua = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    WalletIdAsaas = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NumeroWhatsappLocador = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MensagemPadraoWhatsapp = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ChavePix = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    NomeRecebedorPix = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    CidadeRecebedorPix = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    HostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configuracoes_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GastosManutencao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApartamentoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosManutencao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GastosManutencao_Apartamentos_ApartamentoId",
                        column: x => x.ApartamentoId,
                        principalTable: "Apartamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inquilinos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NomeCompleto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Rg = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OrgaoEmissor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EstadoCivil = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeMoradores = table.Column<int>(type: "INTEGER", nullable: false),
                    DataEntrada = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataVencimentoContrato = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ValorAluguel = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Garagem = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiasAlertaVencimento = table.Column<string>(type: "TEXT", nullable: false),
                    ApartamentoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquilinos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inquilinos_Apartamentos_ApartamentoId",
                        column: x => x.ApartamentoId,
                        principalTable: "Apartamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContratosInquilino",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InquilinoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NomeOriginalArquivo = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TipoConteudo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosInquilino", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratosInquilino_Inquilinos_InquilinoId",
                        column: x => x.InquilinoId,
                        principalTable: "Inquilinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dependentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NomeCompleto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Rg = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    OrgaoEmissor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EstadoCivil = table.Column<int>(type: "INTEGER", nullable: false),
                    InquilinoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dependentes_Inquilinos_InquilinoId",
                        column: x => x.InquilinoId,
                        principalTable: "Inquilinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Faturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MesReferencia = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    ValorAluguel = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ValorAgua = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ValorLuz = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ValorGaragem = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    KwMesAnterior = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    KwAtual = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    KwhValor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    DataLimitePagamento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CodigoPix = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CobrancaAsaasId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    InquilinoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faturas_Inquilinos_InquilinoId",
                        column: x => x.InquilinoId,
                        principalTable: "Inquilinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apartamentos_HostId_Numero_Bloco",
                table: "Apartamentos",
                columns: new[] { "HostId", "Numero", "Bloco" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configuracoes_HostId",
                table: "Configuracoes",
                column: "HostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContratosInquilino_InquilinoId",
                table: "ContratosInquilino",
                column: "InquilinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependentes_Cpf",
                table: "Dependentes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dependentes_InquilinoId",
                table: "Dependentes",
                column: "InquilinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_InquilinoId_MesReferencia",
                table: "Faturas",
                columns: new[] { "InquilinoId", "MesReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GastosManutencao_ApartamentoId",
                table: "GastosManutencao",
                column: "ApartamentoId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Inquilinos_ApartamentoId",
                table: "Inquilinos",
                column: "ApartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquilinos_Cpf",
                table: "Inquilinos",
                column: "Cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "ContratosInquilino");

            migrationBuilder.DropTable(
                name: "Dependentes");

            migrationBuilder.DropTable(
                name: "Faturas");

            migrationBuilder.DropTable(
                name: "GastosManutencao");

            migrationBuilder.DropTable(
                name: "Inquilinos");

            migrationBuilder.DropTable(
                name: "Apartamentos");

            migrationBuilder.DropTable(
                name: "Hosts");
        }
    }
}
