using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

/// <summary>
/// Configuração do mapeamento da entidade <see cref="Apartamento"/> para o banco de dados.
/// Segue o padrão IEntityTypeConfiguration do EF Core para manter o contexto limpo (SRP).
/// </summary>
public class ApartamentoConfiguracao : IEntityTypeConfiguration<Apartamento>
{
    /// <summary>
    /// Configura a tabela, colunas, restrições e relacionamentos da entidade Apartamento.
    /// </summary>
    /// <param name="builder">Construtor de configuração da entidade.</param>
    public void Configure(EntityTypeBuilder<Apartamento> builder)
    {
        // Nome da tabela no banco de dados
        builder.ToTable("Apartamentos");

        // Chave primária
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Número do apartamento — máximo 20 caracteres, obrigatório
        builder.Property(a => a.Numero)
            .HasColumnName("Numero")
            .HasMaxLength(20)
            .IsRequired();

        // Bloco/Torre — máximo 10 caracteres, opcional
        builder.Property(a => a.Bloco)
            .HasColumnName("Bloco")
            .HasMaxLength(10)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        // Status de ocupação
        builder.Property(a => a.Ocupado)
            .HasColumnName("Ocupado")
            .IsRequired()
            .HasDefaultValue(false);

        // Auditorias de data
        builder.Property(a => a.CriadoEm)
            .HasColumnName("CriadoEm")
            .IsRequired();

        builder.Property(a => a.AtualizadoEm)
            .HasColumnName("AtualizadoEm")
            .IsRequired(false);

        // Índice único: não pode existir dois apartamentos com mesmo número e bloco
        builder.HasIndex(a => new { a.Numero, a.Bloco })
            .IsUnique()
            .HasDatabaseName("IX_Apartamentos_Numero_Bloco");

        // =============================================================
        // RELACIONAMENTO: 1 Apartamento -> N Inquilinos
        // Um apartamento pode ter vários inquilinos ao longo do tempo.
        // A deleção do apartamento não deve remover os inquilinos
        // (Restrict) para preservar o histórico.
        // =============================================================
        builder.HasMany(a => a.Inquilinos)
            .WithOne(i => i.Apartamento)
            .HasForeignKey(i => i.ApartamentoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

