using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato específico de repositório para a entidade <see cref="Apartamento"/>.
/// Estende o repositório genérico com operações específicas para apartamentos.
/// </summary>
public interface IApartamentoRepositorio : IRepositorio<Apartamento>
{
    /// <summary>
    /// Verifica se já existe um apartamento com o número e bloco informados.
    /// Utilizado para evitar duplicatas no cadastro.
    /// </summary>
    /// <param name="numero">Número do apartamento.</param>
    /// <param name="bloco">Bloco do apartamento.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Verdadeiro se o apartamento já existir.</returns>
    Task<bool> ExisteAsync(string numero, string bloco, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os apartamentos que estão atualmente desocupados.
    /// Útil para listagem de unidades disponíveis para locação.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista de apartamentos desocupados.</returns>
    Task<IEnumerable<Apartamento>> ObterDesocupadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um apartamento pelo número e bloco.
    /// </summary>
    /// <param name="numero">Número do apartamento.</param>
    /// <param name="bloco">Bloco do apartamento.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>O apartamento encontrado ou nulo.</returns>
    Task<Apartamento?> ObterPorNumeroBlocoAsync(string numero, string bloco, CancellationToken cancellationToken = default);
}

