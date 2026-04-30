using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

/// <summary>
/// Implementação genérica do repositório base seguindo o padrão Repository Pattern.
/// Fornece as operações básicas de CRUD para qualquer entidade que herde de <see cref="EntidadeBase"/>.
/// As implementações específicas herdam desta classe para adicionar queries especializadas.
/// </summary>
/// <typeparam name="T">Tipo da entidade gerenciada pelo repositório.</typeparam>
public abstract class RepositorioBase<T> : IRepositorio<T> where T : EntidadeBase
{
    /// <summary>
    /// Contexto do banco de dados injetado via construtor.
    /// </summary>
    protected readonly AluguelDbContext Contexto;

    /// <summary>
    /// DbSet tipado para a entidade gerenciada.
    /// </summary>
    protected readonly DbSet<T> DbSet;

    /// <summary>
    /// Inicializa o repositório base com o contexto do banco de dados.
    /// </summary>
    /// <param name="contexto">Contexto do EF Core.</param>
    protected RepositorioBase(AluguelDbContext contexto)
    {
        Contexto = contexto;
        DbSet = contexto.Set<T>();
    }

    /// <summary>
    /// Busca uma entidade pelo identificador único (Guid) de forma assíncrona.
    /// </summary>
    public virtual async Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(new object[] { id }, cancellationToken);

    /// <summary>
    /// Retorna todas as entidades do tipo gerenciado de forma assíncrona.
    /// </summary>
    public virtual async Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>
    /// Adiciona uma nova entidade ao contexto de forma assíncrona.
    /// </summary>
    public virtual async Task AdicionarAsync(T entidade, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entidade, cancellationToken);

    /// <summary>
    /// Marca a entidade como modificada no contexto do EF Core.
    /// </summary>
    public virtual void Atualizar(T entidade)
        => DbSet.Update(entidade);

    /// <summary>
    /// Remove uma entidade do contexto de forma síncrona.
    /// </summary>
    public virtual void Remover(T entidade)
        => DbSet.Remove(entidade);

    /// <summary>
    /// Persiste todas as alterações pendentes no banco de dados.
    /// </summary>
    public async Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await Contexto.SaveChangesAsync(cancellationToken);
}

