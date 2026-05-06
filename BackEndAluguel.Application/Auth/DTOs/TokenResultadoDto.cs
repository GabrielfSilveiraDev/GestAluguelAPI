namespace BackEndAluguel.Application.Auth.DTOs;

/// <summary>
/// DTO de resposta para operacoes de autenticacao bem-sucedidas.
/// Contem o token JWT e informacoes do usuario autenticado.
/// </summary>
/// <param name="Token">Token JWT assinado para uso nas requisicoes subsequentes.</param>
/// <param name="Perfil">Perfil do usuario autenticado: "Host" ou "Inquilino".</param>
/// <param name="NomeCompleto">Nome completo do usuario autenticado.</param>
/// <param name="InquilinoId">Identificador do inquilino (nulo para o host).</param>
/// <param name="HostId">Identificador do host (nulo para inquilinos).</param>
/// <param name="Expiracao">Data e hora de expiracao do token (UTC).</param>
public record TokenResultadoDto(
    string Token,
    string Perfil,
    string NomeCompleto,
    Guid? InquilinoId,
    Guid? HostId,
    DateTime Expiracao
);

