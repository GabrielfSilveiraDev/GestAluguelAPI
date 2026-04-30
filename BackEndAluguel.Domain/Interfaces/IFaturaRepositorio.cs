using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;

namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato específico de repositório para a entidade <see cref="Fatura"/>.
/// Estende o repositório genérico com operações específicas para faturas mensais.
/// </summary>
public interface IFaturaRepositorio : IRepositorio<Fatura>
{
    /// <summary>
    /// Verifica se já existe uma fatura para um inquilino em um determinado mês de referência.
    /// Evita duplicidade de faturas para o mesmo período.
    /// </summary>
    /// <param name="inquilinoId">Identificador do inquilino.</param>
    /// <param name="mesReferencia">Mês de referência no formato "MM/AAAA".</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Verdadeiro se já existir uma fatura para o período.</returns>
    Task<bool> ExisteParaMesReferenciaAsync(Guid inquilinoId, string mesReferencia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todas as faturas de um inquilino específico.
    /// </summary>
    /// <param name="inquilinoId">Identificador do inquilino.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista de faturas do inquilino.</returns>
    Task<IEnumerable<Fatura>> ObterPorInquilinoAsync(Guid inquilinoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todas as faturas com um determinado status.
    /// Útil para listar faturas pendentes, atrasadas ou pagas.
    /// </summary>
    /// <param name="status">Status da fatura a ser filtrado.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista de faturas com o status especificado.</returns>
    Task<IEnumerable<Fatura>> ObterPorStatusAsync(StatusFatura status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todas as faturas vencidas (data limite ultrapassada e não pagas).
    /// Utilizado para processamento automático de marcação de atraso.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista de faturas vencidas e não pagas.</returns>
    Task<IEnumerable<Fatura>> ObterFaturasVencidasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna a fatura de um inquilino para um mês de referência específico.
    /// </summary>
    Task<Fatura?> ObterPorInquilinoEMesAsync(Guid inquilinoId, string mesReferencia, CancellationToken cancellationToken = default);

    /// <summary>Retorna todas as faturas de um mes/ano especifico com Include de Inquilino e Apartamento.</summary>
    Task<IEnumerable<Fatura>> ObterPorMesReferenciaComDetalhesAsync(string mesReferencia, CancellationToken cancellationToken = default);

    /// <summary>Retorna faturas pagas agrupadas por ano para o balanco anual.</summary>
    Task<IEnumerable<Fatura>> ObterPagasPorAnoAsync(int ano, CancellationToken cancellationToken = default);

    /// <summary>Busca uma fatura pelo ID externo de cobrança no gateway Asaas.</summary>
    Task<Fatura?> ObterPorCobrancaAsaasIdAsync(string cobrancaAsaasId, CancellationToken cancellationToken = default);
}

