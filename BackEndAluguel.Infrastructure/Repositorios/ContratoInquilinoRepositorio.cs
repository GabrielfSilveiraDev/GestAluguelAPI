using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

/// <summary>
/// Implementacao concreta do repositorio de <see cref="ContratoInquilino"/>.
/// </summary>
public class ContratoInquilinoRepositorio : RepositorioBase<ContratoInquilino>, IContratoInquilinoRepositorio
{
    public ContratoInquilinoRepositorio(AluguelDbContext contexto) : base(contexto) { }

    /// <summary>Retorna todos os contratos de um inquilino, do mais recente ao mais antigo.</summary>
    public async Task<IEnumerable<ContratoInquilino>> ObterPorInquilinoAsync(
        Guid inquilinoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(c => c.InquilinoId == inquilinoId)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(cancellationToken);
}

