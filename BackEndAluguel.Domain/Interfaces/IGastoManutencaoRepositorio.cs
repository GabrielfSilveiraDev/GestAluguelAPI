using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

public interface IGastoManutencaoRepositorio : IRepositorio<GastoManutencao>
{
    Task<IEnumerable<GastoManutencao>> ObterPorApartamentoAsync(Guid apartamentoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GastoManutencao>> ObterPorMesAsync(int ano, int mes, CancellationToken cancellationToken = default);
    Task<IEnumerable<GastoManutencao>> ObterPorAnoAsync(int ano, CancellationToken cancellationToken = default);
}

