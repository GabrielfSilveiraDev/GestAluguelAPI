using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

public class GastoManutencaoRepositorio : RepositorioBase<GastoManutencao>, IGastoManutencaoRepositorio
{
    public GastoManutencaoRepositorio(AluguelDbContext contexto) : base(contexto) { }

    public async Task<IEnumerable<GastoManutencao>> ObterPorApartamentoAsync(Guid apartamentoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(g => g.ApartamentoId == apartamentoId)
            .OrderByDescending(g => g.Data)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<GastoManutencao>> ObterPorMesAsync(int ano, int mes, CancellationToken cancellationToken = default)
    {
        var inicio = new DateOnly(ano, mes, 1);
        var fim = inicio.AddMonths(1).AddDays(-1);
        return await DbSet.AsNoTracking()
            .Include(g => g.Apartamento)
            .Where(g => g.Data >= inicio && g.Data <= fim)
            .OrderBy(g => g.ApartamentoId).ThenBy(g => g.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GastoManutencao>> ObterPorAnoAsync(int ano, CancellationToken cancellationToken = default)
    {
        var inicio = new DateOnly(ano, 1, 1);
        var fim = new DateOnly(ano, 12, 31);
        return await DbSet.AsNoTracking()
            .Include(g => g.Apartamento)
            .Where(g => g.Data >= inicio && g.Data <= fim)
            .OrderBy(g => g.Data)
            .ToListAsync(cancellationToken);
    }
}

