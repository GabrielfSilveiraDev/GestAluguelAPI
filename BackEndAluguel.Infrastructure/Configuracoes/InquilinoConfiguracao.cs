using BackEndAluguel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAluguel.Infrastructure.Configuracoes;

/// <summary>
/// Configuração do mapeamento da entidade <see cref="Inquilino"/> para o banco de dados.
/// Define todas as colunas, restrições, conversões e relacionamentos.
/// </summary>
public class InquilinoConfiguracao : IEntityTypeConfiguration<Inquilino>
{
    /// <summary>
    /// Configura a tabela, colunas, conversões de tipo e relacionamentos da entidade Inquilino.
    /// </summary>
    /// <param name="builder">Construtor de configuração da entidade.</param>
    public void Configure(EntityTypeBuilder<Inquilino> builder)
    {
        // Nome da tabela no banco de dados
        builder.ToTable("Inquilinos");

        // Chave primária
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Nome completo — máximo 200 caracteres, obrigatório
        builder.Property(i => i.NomeCompleto)
            .HasColumnName("NomeCompleto")
            .HasMaxLength(200)
            .IsRequired();

        // CPF — 11 dígitos, único no sistema
        builder.Property(i => i.Cpf)
            .HasColumnName("Cpf")
            .HasMaxLength(11)
            .IsRequired();

        // Índice único para garantir CPF único no banco
        builder.HasIndex(i => i.Cpf)
            .IsUnique()
            .HasDatabaseName("IX_Inquilinos_Cpf");

        // RG — obrigatório (inquilino é também um morador)
        builder.Property(i => i.Rg)
            .HasColumnName("Rg")
            .HasMaxLength(20)
            .IsRequired();

        // Órgão emissor do RG
        builder.Property(i => i.OrgaoEmissor)
            .HasColumnName("OrgaoEmissor")
            .HasMaxLength(20)
            .IsRequired();

        // Telefone de contato
        builder.Property(i => i.Telefone)
            .HasColumnName("Telefone")
            .HasMaxLength(20)
            .IsRequired();

        // Estado civil
        builder.Property(i => i.EstadoCivil)
            .HasColumnName("EstadoCivil")
            .IsRequired();

        // Quantidade de moradores
        builder.Property(i => i.QuantidadeMoradores)
            .HasColumnName("QuantidadeMoradores")
            .IsRequired();

        // Data de nascimento — usada como segundo fator no login do inquilino
        builder.Property(i => i.DataNascimento)
            .HasColumnName("DataNascimento")
            .IsRequired();

        // Datas do contrato — armazenadas como DateOnly no banco
        builder.Property(i => i.DataEntrada)
            .HasColumnName("DataEntrada")
            .IsRequired();

        builder.Property(i => i.DataVencimentoContrato)
            .HasColumnName("DataVencimentoContrato")
            .IsRequired();

        // Valor do aluguel — precisão de 18 dígitos, 2 casas decimais
        builder.Property(i => i.ValorAluguel)
            .HasColumnName("ValorAluguel")
            .HasPrecision(18, 2)
            .IsRequired();

        // Valor da garagem — precisão de 18 dígitos, 2 casas decimais, padrão 0
        builder.Property(i => i.Garagem)
            .HasColumnName("Garagem")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        // DiasAlertaVencimento — armazenada como CSV no banco de dados (ex: "30,60,90").
        // Converte List<int> para string CSV e vice-versa.
        // O ValueComparer garante que o EF Core detecte corretamente mudanças na coleção.
        var diasAlertaProperty = builder.Property(i => i.DiasAlertaVencimento)
            .HasColumnName("DiasAlertaVencimento")
            .HasConversion(
                lista => string.Join(",", lista),
                valor => valor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(int.Parse)
                              .ToList())
            .IsRequired();

        // ValueComparer necessário para o EF Core detectar mudanças em coleções (Change Tracking)
        diasAlertaProperty.Metadata.SetValueComparer(new ValueComparer<List<int>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()));

        // Chave estrangeira para Apartamento
        builder.Property(i => i.ApartamentoId)
            .HasColumnName("ApartamentoId")
            .IsRequired();

        // Auditorias de data
        builder.Property(i => i.CriadoEm)
            .HasColumnName("CriadoEm")
            .IsRequired();

        builder.Property(i => i.AtualizadoEm)
            .HasColumnName("AtualizadoEm")
            .IsRequired(false);

        // Relacionamento: 1 Inquilino -> N Dependentes
        builder.HasMany(i => i.Dependentes)
            .WithOne(d => d.Inquilino)
            .HasForeignKey(d => d.InquilinoId)
            .OnDelete(DeleteBehavior.Cascade);

        // =============================================================
        // RELACIONAMENTO: 1 Inquilino -> N Faturas
        // Um inquilino pode ter várias faturas mensais.
        // A exclusão de um inquilino em cascata remove suas faturas.
        // =============================================================
        builder.HasMany(i => i.Faturas)
            .WithOne(f => f.Inquilino)
            .HasForeignKey(f => f.InquilinoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

