using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

public class GastoManutencaoConfiguracao : IEntityTypeConfiguration<GastoManutencao>
{
    public void Configure(EntityTypeBuilder<GastoManutencao> builder)
    {
        builder.ToTable("GastosManutencao");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.ApartamentoId).IsRequired();
        builder.Property(g => g.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(g => g.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(g => g.Data).IsRequired();
        builder.Property(g => g.Observacao).HasMaxLength(1000).IsRequired(false);
        builder.Property(g => g.CriadoEm).IsRequired();
        builder.Property(g => g.AtualizadoEm).IsRequired(false);

        builder.HasOne(g => g.Apartamento)
            .WithMany(a => a.GastosManutencao)
            .HasForeignKey(g => g.ApartamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

