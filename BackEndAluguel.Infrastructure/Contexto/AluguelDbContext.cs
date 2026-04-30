using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Infrastructure.Configuracoes;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Contexto;

/// <summary>
/// Contexto principal do Entity Framework Core para o sistema de gerenciamento de aluguéis.
/// Centraliza o acesso ao banco de dados e aplica todas as configurações de mapeamento das entidades.
/// 
/// Relações gerenciadas:
/// - Apartamento (1) → Inquilinos (N): Um apartamento possui vários inquilinos ao longo do tempo.
/// - Inquilino (1) → Faturas (N): Um inquilino possui várias faturas mensais.
/// </summary>
public class AluguelDbContext : DbContext
{
    /// <summary>
    /// Inicializa o contexto com as opções de configuração fornecidas via injeção de dependência.
    /// </summary>
    /// <param name="opcoes">Opções de configuração do DbContext (connection string, provider, etc.).</param>
    public AluguelDbContext(DbContextOptions<AluguelDbContext> opcoes) : base(opcoes)
    {
    }

    // =============================================================
    // DbSets — Representam as tabelas no banco de dados
    // =============================================================

    /// <summary>
    /// Representa a tabela "Apartamentos" no banco de dados.
    /// Permite realizar operações CRUD sobre os apartamentos cadastrados.
    /// </summary>
    public DbSet<Apartamento> Apartamentos => Set<Apartamento>();

    /// <summary>
    /// Representa a tabela "Inquilinos" no banco de dados.
    /// Permite realizar operações CRUD sobre os inquilinos cadastrados.
    /// </summary>
    public DbSet<Inquilino> Inquilinos => Set<Inquilino>();

    /// <summary>
    /// Representa a tabela "Faturas" no banco de dados.
    /// Permite realizar operações CRUD sobre as faturas mensais.
    /// </summary>
    public DbSet<Fatura> Faturas => Set<Fatura>();

    /// <summary>
    /// Representa a tabela "Dependentes" no banco de dados.
    /// Permite realizar operações CRUD sobre os dependentes cadastrados.
    /// </summary>
    public DbSet<Dependente> Dependentes => Set<Dependente>();

    /// <summary>
    /// Representa a tabela "Configuracoes" no banco de dados.
    /// Permite realizar operações CRUD sobre as configurações do sistema.
    /// </summary>
    public DbSet<Configuracao> Configuracoes => Set<Configuracao>();

    /// <summary>
    /// Representa a tabela "GastosManutencao" no banco de dados.
    /// </summary>
    public DbSet<GastoManutencao> GastosManutencao => Set<GastoManutencao>();

    /// <summary>
    /// Representa a tabela "Hosts" no banco de dados.
    /// Permite realizar operações CRUD sobre os administradores (locadores) cadastrados.
    /// </summary>
    public DbSet<Host> Hosts => Set<Host>();

    /// <summary>
    /// Representa a tabela "ContratosInquilino" no banco de dados.
    /// </summary>
    public DbSet<ContratoInquilino> ContratosInquilino => Set<ContratoInquilino>();

    /// <summary>
    /// Configura o mapeamento das entidades para o banco de dados aplicando as classes de configuração
    /// do padrão IEntityTypeConfiguration, mantendo a separação de responsabilidades (SRP).
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo de dados do EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente todas as configurações de entidade
        // definidas na assembly da infraestrutura (IEntityTypeConfiguration<T>)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AluguelDbContext).Assembly);

        // Seed do registro singleton de Configuracao
        modelBuilder.Entity<Configuracao>().HasData(new
        {
            Id = ConfiguracaoConfiguracao.ConfiguracaoId,
            KwhValor = 0.0m,
            ValorAgua = 0.0m,
            CriadoEm = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AtualizadoEm = (DateTime?)null
        });
    }
}
