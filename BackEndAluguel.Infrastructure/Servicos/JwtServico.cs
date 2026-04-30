using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackEndAluguel.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementacao concreta do servico de geracao de tokens JWT.
/// Utiliza a chave secreta, issuer e audience definidos no appsettings.json.
/// </summary>
public class JwtServico : IJwtServico
{
    private readonly IConfiguration _configuracao;

    /// <summary>Inicializa o servico JWT com as configuracoes da aplicacao.</summary>
    public JwtServico(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    /// <summary>
    /// Gera um token JWT para o administrador (host/locador) autenticado pelo banco de dados.
    /// </summary>
    public (string Token, DateTime Expiracao) GerarTokenHost(Guid hostId, string nomeCompleto, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, hostId.ToString()),
            new(ClaimTypes.Name, nomeCompleto),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, "Host"),
            new("hostId", hostId.ToString()),
            new(JwtRegisteredClaimNames.Sub, hostId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return GerarToken(claims);
    }

    /// <summary>
    /// Gera um token JWT para um inquilino autenticado.
    /// Retorna o token assinado e a data de expiracao UTC.
    /// </summary>
    public (string Token, DateTime Expiracao) GerarTokenInquilino(Guid inquilinoId, string nomeCompleto, string cpf)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, inquilinoId.ToString()),
            new(ClaimTypes.Name, nomeCompleto),
            new(ClaimTypes.Role, "Inquilino"),
            new("cpf", cpf),
            new("inquilinoId", inquilinoId.ToString()),
            new(JwtRegisteredClaimNames.Sub, inquilinoId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return GerarToken(claims);
    }

    /// <summary>
    /// Metodo auxiliar que assina e serializa o token JWT com as claims fornecidas.
    /// Utiliza HMAC-SHA256 com a chave secreta do appsettings.json.
    /// </summary>
    private (string Token, DateTime Expiracao) GerarToken(IEnumerable<Claim> claims)
    {
        var chaveSecreta = _configuracao["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Chave secreta JWT nao configurada.");

        var issuer = _configuracao["Jwt:Issuer"] ?? "GestAluguelAPI";
        var audience = _configuracao["Jwt:Audience"] ?? "GestAluguelFrontEnd";
        var horasExpiracao = int.TryParse(_configuracao["Jwt:ExpiracaoHoras"], out var h) ? h : 24;
        var expiracao = DateTime.UtcNow.AddHours(horasExpiracao);

        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
        var credenciais = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}

