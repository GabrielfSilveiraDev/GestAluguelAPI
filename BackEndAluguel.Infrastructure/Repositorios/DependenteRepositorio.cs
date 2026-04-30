using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

public class DependenteRepositorio : RepositorioBase<Dependente>, IDependenteRepositorio
{
    public DependenteRepositorio(AluguelDbContext contexto) : base(contexto) { }

    public async Task<IEnumerable<Dependente>> ObterPorInquilinoAsync(Guid inquilinoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.InquilinoId == inquilinoId)
            .OrderBy(d => d.NomeCompleto)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistePorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(d => d.Cpf == cpf, cancellationToken);
}

