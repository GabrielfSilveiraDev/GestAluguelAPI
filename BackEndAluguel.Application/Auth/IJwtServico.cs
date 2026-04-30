namespace BackEndAluguel.Application.Auth;

/// <summary>
/// Contrato para geracao de tokens JWT para autenticacao de host e inquilinos.
/// A implementacao concreta fica na camada Infrastructure.
/// Retorna o token e a data de expiracao para evitar dependencia de IConfiguration na camada Application.
/// </summary>
public interface IJwtServico
{
    /// <summary>
    /// Gera um token JWT para o administrador (host/locador) autenticado pelo banco de dados.
    /// </summary>
    /// <param name="hostId">Identificador único do host.</param>
    /// <param name="nomeCompleto">Nome completo do host.</param>
    /// <param name="email">E-mail do host.</param>
    /// <returns>Token JWT assinado e data/hora de expiracao (UTC).</returns>
    (string Token, DateTime Expiracao) GerarTokenHost(Guid hostId, string nomeCompleto, string email);

    /// <summary>
    /// Gera um token JWT para um inquilino autenticado.
    /// O token contera o InquilinoId, nome e CPF como claims.
    /// </summary>
    /// <param name="inquilinoId">Identificador unico do inquilino.</param>
    /// <param name="nomeCompleto">Nome completo do inquilino.</param>
    /// <param name="cpf">CPF do inquilino.</param>
    /// <returns>Token JWT assinado e data/hora de expiracao (UTC).</returns>
    (string Token, DateTime Expiracao) GerarTokenInquilino(Guid inquilinoId, string nomeCompleto, string cpf);
}

