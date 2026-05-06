using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

/// <summary>
/// Configuração do mapeamento da entidade <see cref="Fatura"/> para o banco de dados.
/// Define colunas, precisões monetárias, enumerações e relacionamentos.
/// </summary>
public class FaturaConfiguracao : IEntityTypeConfiguration<Fatura>
{
    /// <summary>
    /// Configura a tabela, colunas, conversões de tipo e relacionamentos da entidade Fatura.
    /// </summary>
    /// <param name="builder">Construtor de configuração da entidade.</param>
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        // Nome da tabela no banco de dados
        builder.ToTable("Faturas");

        // Chave primária
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Mês de referência no formato "MM/AAAA"
        builder.Property(f => f.MesReferencia)
            .HasColumnName("MesReferencia")
            .HasMaxLength(7)
            .IsRequired();

        // Valores monetários — precisão de 18 dígitos, 2 casas decimais
        builder.Property(f => f.ValorAluguel)
            .HasColumnName("ValorAluguel")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.ValorAgua)
            .HasColumnName("ValorAgua")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(f => f.ValorLuz)
            .HasColumnName("ValorLuz")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(f => f.ValorGaragem)
            .HasColumnName("ValorGaragem")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        // Campos de leitura de kWh (opcionais)
        builder.Property(f => f.KwMesAnterior)
            .HasColumnName("KwMesAnterior")
            .HasPrecision(18, 3)
            .IsRequired(false);

        builder.Property(f => f.KwAtual)
            .HasColumnName("KwAtual")
            .HasPrecision(18, 3)
            .IsRequired(false);

        builder.Property(f => f.KwhValor)
            .HasColumnName("KwhValor")
            .HasPrecision(18, 4)
            .IsRequired(false);

        // KwConsumidos e calculado, nao e mapeado para o banco
        builder.Ignore(f => f.KwConsumidos);

        // Datas — armazenadas como DateOnly
        builder.Property(f => f.DataLimitePagamento)
            .HasColumnName("DataLimitePagamento")
            .IsRequired();

        builder.Property(f => f.DataPagamento)
            .HasColumnName("DataPagamento")
            .IsRequired(false);

        // Código/link PIX — opcional, máximo 500 caracteres
        builder.Property(f => f.CodigoPix)
            .HasColumnName("CodigoPix")
            .HasMaxLength(500)
            .IsRequired(false);

        // Identificador externo da cobranca no Asaas (para correlacao via webhook)
        builder.Property(f => f.CobrancaAsaasId)
            .HasColumnName("CobrancaAsaasId")
            .HasMaxLength(100)
            .IsRequired(false);

        // Status da fatura armazenado como inteiro no banco de dados.
        // Não usamos HasDefaultValue pois o status é sempre definido explicitamente
        // no construtor da entidade Fatura (StatusFatura.Pendente = 1).
        builder.Property(f => f.Status)
            .HasColumnName("Status")
            .HasConversion<int>()
            .IsRequired();

        // Chave estrangeira para Inquilino
        builder.Property(f => f.InquilinoId)
            .HasColumnName("InquilinoId")
            .IsRequired();

        // Auditorias de data
        builder.Property(f => f.CriadoEm)
            .HasColumnName("CriadoEm")
            .IsRequired();

        builder.Property(f => f.AtualizadoEm)
            .HasColumnName("AtualizadoEm")
            .IsRequired(false);

        // Índice único: um inquilino só pode ter uma fatura por mês de referência
        builder.HasIndex(f => new { f.InquilinoId, f.MesReferencia })
            .IsUnique()
            .HasDatabaseName("IX_Faturas_InquilinoId_MesReferencia");
    }
}

