using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

public class DependenteConfiguracao : IEntityTypeConfiguration<Dependente>
{
    public void Configure(EntityTypeBuilder<Dependente> builder)
    {
        builder.ToTable("Dependentes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Cpf).HasMaxLength(11).IsRequired();
        builder.Property(d => d.Rg).HasMaxLength(20).IsRequired(false);
        builder.Property(d => d.OrgaoEmissor).HasMaxLength(20).IsRequired(false);
        builder.Property(d => d.DataNascimento).IsRequired();
        builder.Property(d => d.Telefone).HasMaxLength(20).IsRequired(false);
        builder.Property(d => d.EstadoCivil).HasConversion<int>().IsRequired();
        builder.Property(d => d.InquilinoId).IsRequired();
        builder.Property(d => d.CriadoEm).IsRequired();
        builder.Property(d => d.AtualizadoEm).IsRequired(false);

        builder.HasIndex(d => d.Cpf).IsUnique().HasDatabaseName("IX_Dependentes_Cpf");
    }
}

