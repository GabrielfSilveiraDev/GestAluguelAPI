using BackEndAluguel.Application.Comum;
using BackEndAluguel.Domain.Entidades;
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
    private readonly ITenantContexto? _tenantContexto;

    /// <summary>
    /// Inicializa o contexto com as opções de configuração fornecidas via injeção de dependência.
    /// </summary>
    public AluguelDbContext(DbContextOptions<AluguelDbContext> opcoes, ITenantContexto? tenantContexto = null)
        : base(opcoes)
    {
        _tenantContexto = tenantContexto;
    }

    /// <summary>
    /// Retorna o HostId do tenant atual, ou null se não houver contexto de tenant (ex: testes).
    /// </summary>
    private Guid? HostIdAtual => _tenantContexto?.ObterHostId();

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

        // Filtros globais de multi-tenancy — cada query retorna apenas os dados do host autenticado
        modelBuilder.Entity<Apartamento>()
            .HasQueryFilter(a => HostIdAtual == null || a.HostId == HostIdAtual);
        modelBuilder.Entity<Inquilino>()
            .HasQueryFilter(i => HostIdAtual == null || i.HostId == HostIdAtual);
        modelBuilder.Entity<Fatura>()
            .HasQueryFilter(f => HostIdAtual == null || f.HostId == HostIdAtual);
        modelBuilder.Entity<Configuracao>()
            .HasQueryFilter(c => HostIdAtual == null || c.HostId == HostIdAtual);
        modelBuilder.Entity<GastoManutencao>()
            .HasQueryFilter(g => HostIdAtual == null || g.Apartamento!.HostId == HostIdAtual);
    }
}
