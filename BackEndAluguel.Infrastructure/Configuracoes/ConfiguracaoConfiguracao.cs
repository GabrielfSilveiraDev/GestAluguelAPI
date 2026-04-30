using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

public class ConfiguracaoConfiguracao : IEntityTypeConfiguration<Configuracao>
{
    // ID fixo para o registro singleton
    public static readonly Guid ConfiguracaoId = new("00000000-0000-0000-0000-000000000001");

    public void Configure(EntityTypeBuilder<Configuracao> builder)
    {
        builder.ToTable("Configuracoes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.KwhValor).HasPrecision(18, 4).IsRequired();
        builder.Property(c => c.ValorAgua).HasPrecision(18, 2).IsRequired();
        builder.Property(c => c.WalletIdAsaas).HasMaxLength(100).IsRequired(false);
        builder.Property(c => c.CriadoEm).IsRequired();
        builder.Property(c => c.AtualizadoEm).IsRequired(false);
    }
}

