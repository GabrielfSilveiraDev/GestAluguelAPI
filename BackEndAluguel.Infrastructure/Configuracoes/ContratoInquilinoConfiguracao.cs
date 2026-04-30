using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

/// <summary>
/// Configuracao EF Core para a entidade <see cref="ContratoInquilino"/>.
/// Define o mapeamento da tabela de contratos assinados no banco de dados.
/// </summary>
public class ContratoInquilinoConfiguracao : IEntityTypeConfiguration<ContratoInquilino>
{
    /// <summary>Configura a tabela, colunas e relacionamentos da entidade ContratoInquilino.</summary>
    public void Configure(EntityTypeBuilder<ContratoInquilino> builder)
    {
        builder.ToTable("ContratosInquilino");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.InquilinoId).IsRequired();

        builder.Property(c => c.NomeOriginalArquivo)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(c => c.CaminhoArquivo)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.TipoConteudo)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.TamanhoBytes).IsRequired();

        builder.Property(c => c.Descricao)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.CriadoEm).IsRequired();
        builder.Property(c => c.AtualizadoEm).IsRequired(false);

        // Relacionamento: N contratos -> 1 Inquilino
        builder.HasOne(c => c.Inquilino)
            .WithMany()
            .HasForeignKey(c => c.InquilinoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indice para busca rapida por inquilino
        builder.HasIndex(c => c.InquilinoId)
            .HasDatabaseName("IX_ContratosInquilino_InquilinoId");
    }
}

