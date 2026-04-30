using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

/// <summary>
/// Implementação concreta do repositório de hosts.
/// Herda as operações CRUD genéricas do <see cref="RepositorioBase{T}"/>.
/// </summary>
public class HostRepositorio : RepositorioBase<Host>, IHostRepositorio
{
    public HostRepositorio(AluguelDbContext contexto) : base(contexto) { }

    /// <inheritdoc />
    public async Task<Host?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            h => h.Email == email.ToLowerInvariant(), cancellationToken);

    /// <inheritdoc />
    public async Task<Host?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(h => h.Cpf == cpf, cancellationToken);

    /// <inheritdoc />
    public async Task<Host?> ObterPorTokenConfirmacaoAsync(string token, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(h => h.TokenConfirmacao == token, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(h => h.Email == email.ToLowerInvariant(), cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(h => h.Cpf == cpf, cancellationToken);
}

