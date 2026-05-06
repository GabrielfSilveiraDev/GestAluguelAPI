using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

/// <summary>
/// Implementação concreta do repositório de <see cref="Fatura"/>.
/// Herda as operações genéricas do <see cref="RepositorioBase{T}"/> e adiciona
/// queries específicas para faturas mensais.
/// </summary>
public class FaturaRepositorio : RepositorioBase<Fatura>, IFaturaRepositorio
{
    /// <summary>
    /// Inicializa o repositório com o contexto do banco de dados.
    /// </summary>
    public FaturaRepositorio(AluguelDbContext contexto) : base(contexto)
    {
    }

    /// <summary>
    /// Verifica se já existe uma fatura para o inquilino no mês de referência informado.
    /// </summary>
    public async Task<bool> ExisteParaMesReferenciaAsync(
        Guid inquilinoId, string mesReferencia, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            f => f.InquilinoId == inquilinoId && f.MesReferencia == mesReferencia.Trim(),
            cancellationToken);

    /// <summary>
    /// Busca uma fatura pelo ID, incluindo o Inquilino e o Apartamento para enriquecer o DTO.
    /// </summary>
    public override async Task<Fatura?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(f => f.Inquilino)
                .ThenInclude(i => i!.Apartamento)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    /// <summary>
    /// Retorna todas as faturas de um inquilino específico, ordenadas por mês.
    /// </summary>
    public async Task<IEnumerable<Fatura>> ObterPorInquilinoAsync(
        Guid inquilinoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(f => f.Inquilino)
                .ThenInclude(i => i!.Apartamento)
            .Where(f => f.InquilinoId == inquilinoId)
            .OrderByDescending(f => f.DataLimitePagamento)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retorna todas as faturas com o status informado.
    /// </summary>
    public async Task<IEnumerable<Fatura>> ObterPorStatusAsync(
        StatusFatura status, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(f => f.Status == status)
            .OrderBy(f => f.DataLimitePagamento)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retorna todas as faturas vencidas: data limite ultrapassada e status diferente de Pago.
    /// Utilizado para o processamento automático de marcação de atraso.
    /// </summary>
    public async Task<IEnumerable<Fatura>> ObterFaturasVencidasAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return await DbSet
            .Where(f => f.Status != StatusFatura.Pago && f.DataLimitePagamento < hoje)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca a fatura de um inquilino para um mês de referência específico.
    /// </summary>
    public async Task<Fatura?> ObterPorInquilinoEMesAsync(
        Guid inquilinoId, string mesReferencia, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            f => f.InquilinoId == inquilinoId && f.MesReferencia == mesReferencia.Trim(),
            cancellationToken);

    public async Task<IEnumerable<Fatura>> ObterPorMesReferenciaComDetalhesAsync(
        string mesReferencia, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(f => f.Inquilino)
                .ThenInclude(i => i!.Apartamento)
            .Where(f => f.MesReferencia == mesReferencia.Trim())
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Fatura>> ObterPagasPorAnoAsync(int ano, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(f => f.Inquilino)
                .ThenInclude(i => i!.Apartamento)
            .Where(f => f.Status == StatusFatura.Pago && f.MesReferencia.EndsWith($"/{ano}"))
            .OrderBy(f => f.MesReferencia)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Busca uma fatura pelo ID externo de cobranca no Asaas.
    /// Utilizado pelo webhook de confirmacao de pagamento PIX.
    /// </summary>
    public async Task<Fatura?> ObterPorCobrancaAsaasIdAsync(string cobrancaAsaasId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            f => f.CobrancaAsaasId == cobrancaAsaasId,
            cancellationToken);
}

