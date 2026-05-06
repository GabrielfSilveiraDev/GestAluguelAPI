using System.Security.Claims;
using BackEndAluguel.Application.Comum;
using Microsoft.AspNetCore.Http;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementação de <see cref="ITenantContexto"/> que extrai o HostId
/// do token JWT da requisição HTTP atual via <see cref="IHttpContextAccessor"/>.
/// Quando não há contexto HTTP (background services) ou o JWT não possui a claim
/// <c>hostId</c> (ex.: token de inquilino), retorna <c>null</c> — sinalizando que
/// o filtro de tenant não deve ser aplicado.
/// </summary>
public class TenantContexto : ITenantContexto
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContexto(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? ObterHostId()
    {
        var claim = _httpContextAccessor.HttpContext?
            .User?.FindFirst("hostId")?.Value;

        return claim is not null && Guid.TryParse(claim, out var hostId)
            ? hostId
            : null;
    }
}

