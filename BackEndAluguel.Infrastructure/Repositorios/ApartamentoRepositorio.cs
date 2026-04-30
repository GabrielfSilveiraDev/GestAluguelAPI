using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

/// <summary>
/// Implementação concreta do repositório de <see cref="Apartamento"/>.
/// Herda as operações genéricas do <see cref="RepositorioBase{T}"/> e adiciona
/// queries específicas para apartamentos.
/// </summary>
public class ApartamentoRepositorio : RepositorioBase<Apartamento>, IApartamentoRepositorio
{
    /// <summary>
    /// Inicializa o repositório com o contexto do banco de dados.
    /// </summary>
    public ApartamentoRepositorio(AluguelDbContext contexto) : base(contexto)
    {
    }

    /// <summary>
    /// Verifica se já existe um apartamento com o número e bloco informados.
    /// </summary>
    public async Task<bool> ExisteAsync(string numero, string bloco, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            a => a.Numero == numero.Trim().ToUpper() && a.Bloco == bloco.Trim().ToUpper(),
            cancellationToken);

    /// <summary>
    /// Retorna todos os apartamentos que estão desocupados.
    /// </summary>
    public async Task<IEnumerable<Apartamento>> ObterDesocupadosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(a => !a.Ocupado)
            .OrderBy(a => a.Bloco)
            .ThenBy(a => a.Numero)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Busca um apartamento pelo número e bloco.
    /// </summary>
    public async Task<Apartamento?> ObterPorNumeroBlocoAsync(string numero, string bloco, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            a => a.Numero == numero.Trim().ToUpper() && a.Bloco == bloco.Trim().ToUpper(),
            cancellationToken);
}

