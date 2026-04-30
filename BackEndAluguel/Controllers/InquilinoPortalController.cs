using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Contratos.Consultas;
using BackEndAluguel.Application.Contratos.DTOs;
using BackEndAluguel.Application.Faturas.Consultas;
using BackEndAluguel.Application.Faturas.DTOs;
using BackEndAluguel.Application.Inquilinos.Consultas;
using BackEndAluguel.Application.Inquilinos.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller do portal do inquilino.
/// Todos os endpoints exigem autenticacao com perfil "Inquilino".
/// O InquilinoId e extraido automaticamente do token JWT — o inquilino so acessa seus proprios dados.
/// </summary>
[ApiController]
[Route("api/portal")]
[Produces("application/json")]
[Authorize(Roles = "Inquilino")]
public class InquilinoPortalController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Inicializa o controller com o mediator injetado.</summary>
    public InquilinoPortalController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Retorna os dados do inquilino autenticado.
    /// </summary>
    /// <response code="200">Dados do inquilino.</response>
    /// <response code="404">Inquilino nao encontrado.</response>
    [HttpGet("meus-dados")]
    [ProducesResponseType(typeof(RespostaApi<InquilinoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MeusDados(CancellationToken cancellationToken)
    {
        var inquilinoId = ObterInquilinoIdDoToken();
        var resultado = await _mediator.Send(new ObterInquilinoPorIdConsulta(inquilinoId), cancellationToken);

        if (resultado is null)
            return NotFound(RespostaErro.Criar("Inquilino nao encontrado."));

        return Ok(RespostaApi<InquilinoDto>.Ok(resultado));
    }

    /// <summary>
    /// Retorna todas as faturas do inquilino autenticado, ordenadas por data.
    /// </summary>
    /// <response code="200">Lista de faturas do inquilino.</response>
    [HttpGet("faturas")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<FaturaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MinhasFaturas(CancellationToken cancellationToken)
    {
        var inquilinoId = ObterInquilinoIdDoToken();
        var resultado = await _mediator.Send(new ListarFaturasPorInquilinoConsulta(inquilinoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<FaturaDto>>.Ok(resultado));
    }

    /// <summary>
    /// Retorna todos os contratos assinados pelo inquilino autenticado.
    /// </summary>
    /// <response code="200">Lista de contratos do inquilino.</response>
    [HttpGet("contratos")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<ContratoInquilinoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MeusContratos(CancellationToken cancellationToken)
    {
        var inquilinoId = ObterInquilinoIdDoToken();
        var resultado = await _mediator.Send(new ListarContratosPorInquilinoConsulta(inquilinoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<ContratoInquilinoDto>>.Ok(resultado));
    }

    /// <summary>
    /// Extrai o InquilinoId do claim do token JWT autenticado.
    /// A claim "inquilinoId" e inserida pelo JwtServico no momento do login.
    /// </summary>
    private Guid ObterInquilinoIdDoToken()
    {
        var claim = User.FindFirst("inquilinoId")
            ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim is null || !Guid.TryParse(claim.Value, out var inquilinoId))
            throw new InvalidOperationException("Token invalido: InquilinoId nao encontrado.");

        return inquilinoId;
    }
}

