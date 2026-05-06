using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

public class ConfiguracaoConfiguracao : IEntityTypeConfiguration<Configuracao>
{
    public void Configure(EntityTypeBuilder<Configuracao> builder)
    {
        builder.ToTable("Configuracoes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.KwhValor).HasPrecision(18, 4).IsRequired();
        builder.Property(c => c.ValorAgua).HasPrecision(18, 2).IsRequired();
        builder.Property(c => c.WalletIdAsaas).HasMaxLength(100).IsRequired(false);

        // WhatsApp
        builder.Property(c => c.NumeroWhatsappLocador).HasMaxLength(20).IsRequired(false);
        builder.Property(c => c.MensagemPadraoWhatsapp).HasMaxLength(1000).IsRequired(false);

        // PIX nativo
        builder.Property(c => c.ChavePix).HasMaxLength(150).IsRequired(false);
        builder.Property(c => c.NomeRecebedorPix).HasMaxLength(25).IsRequired(false);
        builder.Property(c => c.CidadeRecebedorPix).HasMaxLength(15).IsRequired(false);

        // HostId — chave de isolamento multi-tenant (1 Configuracao por Host)
        builder.Property(c => c.HostId)
            .HasColumnName("HostId")
            .IsRequired();

        // FK para a tabela Hosts
        builder.HasOne<BackEndAluguel.Domain.Entidades.Host>()
            .WithMany()
            .HasForeignKey(c => c.HostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único: um registro de configuração por host
        builder.HasIndex(c => c.HostId)
            .IsUnique()
            .HasDatabaseName("IX_Configuracoes_HostId");

        builder.Property(c => c.CriadoEm).IsRequired();
        builder.Property(c => c.AtualizadoEm).IsRequired(false);
    }
}

