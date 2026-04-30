using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato especifico de repositorio para a entidade <see cref="ContratoInquilino"/>.
/// Permite gerenciar os arquivos de contrato assinados por inquilinos.
/// </summary>
public interface IContratoInquilinoRepositorio : IRepositorio<ContratoInquilino>
{
    /// <summary>Retorna todos os contratos de um inquilino especifico, do mais recente ao mais antigo.</summary>
    Task<IEnumerable<ContratoInquilino>> ObterPorInquilinoAsync(Guid inquilinoId, CancellationToken cancellationToken = default);
}

