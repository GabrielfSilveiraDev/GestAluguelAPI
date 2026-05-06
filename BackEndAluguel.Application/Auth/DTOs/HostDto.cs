namespace BackEndAluguel.Application.Auth.DTOs;

/// <summary>
/// DTO de retorno com os dados do host após registro ou consulta de perfil.
/// </summary>
/// <param name="Id">Identificador único do host.</param>
/// <param name="NomeCompleto">Nome completo do host.</param>
/// <param name="Cpf">CPF do host (somente dígitos).</param>
/// <param name="DataNascimento">Data de nascimento do host.</param>
/// <param name="Email">E-mail do host.</param>
/// <param name="EmailConfirmado">Indica se o e-mail foi confirmado.</param>
/// <param name="CriadoEm">Data de criação da conta.</param>
public record HostDto(
    Guid Id,
    string NomeCompleto,
    string Cpf,
    DateOnly DataNascimento,
    string Email,
    bool EmailConfirmado,
    DateTime CriadoEm
);
