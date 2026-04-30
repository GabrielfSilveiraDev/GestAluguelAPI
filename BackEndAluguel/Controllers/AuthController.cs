using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Auth.Comandos;
using BackEndAluguel.Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsavel pela autenticacao e registro de usuarios na plataforma GestAluguel.
/// Oferece endpoints para registro do host, login do host e login dos inquilinos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Inicializa o controller com o mediator injetado.</summary>
    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Registra um novo host/locador na plataforma.
    /// Cria a conta com e-mail pendente de confirmação e envia o link de ativação por e-mail.
    /// </summary>
    /// <param name="corpo">Dados do novo host: nome, CPF, data de nascimento, e-mail e senha.</param>
    /// <response code="201">Host registrado com sucesso. Aguarda confirmação por e-mail.</response>
    /// <response code="400">Dados inválidos, CPF ou e-mail já cadastrado, senha muito curta.</response>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(RespostaApi<HostDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarHost([FromBody] RegistrarHostCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new RegistrarHostComando(corpo.NomeCompleto, corpo.Cpf, corpo.DataNascimento, corpo.Email, corpo.Senha),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            RespostaApi<HostDto>.Ok(resultado, "Conta criada! Verifique seu e-mail para confirmar o cadastro."));
    }

    /// <summary>
    /// Confirma o e-mail do host a partir do token recebido no e-mail de confirmação.
    /// O frontend deve chamar este endpoint ao exibir a página de confirmação.
    /// </summary>
    /// <param name="corpo">Token de confirmação.</param>
    /// <response code="200">E-mail confirmado. Conta ativa.</response>
    /// <response code="400">Token inválido, já utilizado ou expirado.</response>
    [HttpPost("confirmar-email")]
    [ProducesResponseType(typeof(RespostaApi<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ConfirmarEmailHostComando(corpo.Token), cancellationToken);
        return Ok(RespostaApi<bool>.Ok(resultado, "E-mail confirmado com sucesso! Agora você pode fazer login."));
    }

    /// <summary>
    /// Autentica o administrador (host/locador) pelo e-mail e senha.
    /// Retorna um token JWT com perfil "Host" valido pelo tempo configurado.
    /// </summary>
    /// <param name="corpo">E-mail e senha do host.</param>
    /// <response code="200">Login realizado. Token JWT retornado.</response>
    /// <response code="400">Credenciais inválidas ou conta não confirmada.</response>
    [HttpPost("host")]
    [ProducesResponseType(typeof(RespostaApi<TokenResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginHost([FromBody] LoginHostCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new LoginHostComando(corpo.Email, corpo.Senha), cancellationToken);
        return Ok(RespostaApi<TokenResultadoDto>.Ok(resultado, "Login realizado com sucesso."));
    }

    /// <summary>
    /// Autentica um inquilino pelo CPF e data de nascimento.
    /// Retorna um token JWT com perfil "Inquilino" contendo o InquilinoId como claim.
    /// </summary>
    /// <param name="corpo">CPF e data de nascimento do inquilino.</param>
    /// <response code="200">Login realizado. Token JWT retornado.</response>
    /// <response code="400">CPF ou data de nascimento incorretos.</response>
    [HttpPost("inquilino")]
    [ProducesResponseType(typeof(RespostaApi<TokenResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginInquilino([FromBody] LoginInquilinoCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new LoginInquilinoComando(corpo.Cpf, corpo.DataNascimento), cancellationToken);
        return Ok(RespostaApi<TokenResultadoDto>.Ok(resultado, "Login realizado com sucesso."));
    }
}

// ─── Corpos das requisicoes ───────────────────────────────────────────────────

/// <summary>Corpo da requisicao de registro de host.</summary>
public record RegistrarHostCorpo(
    /// <summary>Nome completo do host.</summary>
    string NomeCompleto,
    /// <summary>CPF do host (com ou sem formatação).</summary>
    string Cpf,
    /// <summary>Data de nascimento do host.</summary>
    DateOnly DataNascimento,
    /// <summary>E-mail do host. Será utilizado como login.</summary>
    string Email,
    /// <summary>Senha (mínimo 6 caracteres). Será criptografada antes de armazenar.</summary>
    string Senha
);

/// <summary>Corpo da requisicao de confirmacao de e-mail.</summary>
public record ConfirmarEmailCorpo(
    /// <summary>Token de confirmação recebido por e-mail.</summary>
    string Token
);

/// <summary>Corpo da requisicao de login do host.</summary>
public record LoginHostCorpo(
    /// <summary>E-mail cadastrado do host.</summary>
    string Email,
    /// <summary>Senha do host.</summary>
    string Senha
);

/// <summary>Corpo da requisicao de login do inquilino.</summary>
public record LoginInquilinoCorpo(
    /// <summary>CPF do inquilino (com ou sem formatacao).</summary>
    string Cpf,
    /// <summary>Data de nascimento do inquilino (segundo fator de autenticacao).</summary>
    DateOnly DataNascimento
);



