namespace BackEndAluguel.Application.Comum;

/// <summary>
/// Contrato para obtenção do identificador do host (locador) autenticado na requisição atual.
/// Utilizado para implementar multi-tenancy — cada host só enxerga seus próprios dados.
/// </summary>
public interface ITenantContexto
{
    /// <summary>
    /// Retorna o HostId do locador autenticado extraído do token JWT.
    /// Retorna <c>null</c> quando não há host autenticado (ex.: background services,
    /// portal do inquilino ou rotas de autenticação).
    /// </summary>
    Guid? ObterHostId();
}

