using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato do repositório de hosts (administradores/locadores).
/// </summary>
public interface IHostRepositorio : IRepositorio<Host>
{
    /// <summary>Busca um host pelo e-mail (case-insensitive).</summary>
    Task<Host?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Busca um host pelo CPF (somente dígitos).</summary>
    Task<Host?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);

    /// <summary>Busca um host pelo token de confirmação de e-mail.</summary>
    Task<Host?> ObterPorTokenConfirmacaoAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe um host com o e-mail informado.</summary>
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe um host com o CPF informado.</summary>
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default);
}

