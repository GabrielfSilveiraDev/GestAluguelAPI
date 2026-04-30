using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

/// <summary>
/// Configuração de mapeamento EF Core para a entidade <see cref="Host"/>.
/// Define a tabela "Hosts" com todas as colunas e constraints.
/// </summary>
public class HostConfiguracao : IEntityTypeConfiguration<Host>
{
    public void Configure(EntityTypeBuilder<Host> builder)
    {
        builder.ToTable("Hosts");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.NomeCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Cpf)
            .IsRequired()
            .HasMaxLength(11)
            .IsFixedLength();

        builder.Property(h => h.DataNascimento)
            .IsRequired();

        builder.Property(h => h.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.SenhaHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(h => h.EmailConfirmado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(h => h.TokenConfirmacao)
            .HasMaxLength(100);

        builder.Property(h => h.TokenExpiracaoConfirmacao);

        builder.Property(h => h.CriadoEm)
            .IsRequired();

        builder.Property(h => h.AtualizadoEm);

        // Índices únicos para evitar duplicação
        builder.HasIndex(h => h.Email).IsUnique();
        builder.HasIndex(h => h.Cpf).IsUnique();
        builder.HasIndex(h => h.TokenConfirmacao).IsUnique().HasFilter("[TokenConfirmacao] IS NOT NULL");
    }
}

