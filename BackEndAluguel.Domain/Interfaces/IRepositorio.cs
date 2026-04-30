using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato genérico de repositório seguindo o princípio da Inversão de Dependência (SOLID - DIP).
/// Define as operações básicas de persistência (CRUD) para qualquer entidade que herde de <see cref="EntidadeBase"/>.
/// As implementações concretas ficam na camada de Infraestrutura.
/// </summary>
/// <typeparam name="T">Tipo da entidade que deve herdar de <see cref="EntidadeBase"/>.</typeparam>
public interface IRepositorio<T> where T : EntidadeBase
{
    /// <summary>
    /// Busca uma entidade pelo seu identificador único (Guid).
    /// </summary>
    /// <param name="id">Identificador único da entidade.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>A entidade encontrada ou nulo se não existir.</returns>
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todas as entidades do tipo especificado.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista com todas as entidades.</returns>
    Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova entidade ao repositório.
    /// </summary>
    /// <param name="entidade">Entidade a ser adicionada.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    Task AdicionarAsync(T entidade, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados de uma entidade existente no repositório.
    /// </summary>
    /// <param name="entidade">Entidade com os dados atualizados.</param>
    void Atualizar(T entidade);

    /// <summary>
    /// Remove uma entidade do repositório.
    /// </summary>
    /// <param name="entidade">Entidade a ser removida.</param>
    void Remover(T entidade);

    /// <summary>
    /// Persiste todas as alterações pendentes no banco de dados de forma assíncrona.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Número de registros afetados.</returns>
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

