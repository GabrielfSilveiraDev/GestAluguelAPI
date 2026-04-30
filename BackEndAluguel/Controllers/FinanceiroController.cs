using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Financeiro.Consultas;
using BackEndAluguel.Application.Financeiro.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsavel pelos endpoints da area financeira.
/// Permite visualizar o balanco de receitas e gastos por mes e por apartamento.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinanceiroController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Inicializa o controller com o mediator injetado.</summary>
    public FinanceiroController(IMediator mediator) { _mediator = mediator; }

    /// <summary>
    /// Retorna o balanco financeiro detalhado de um mes especifico.
    /// Inclui receitas (faturas pagas) e gastos de manutencao por apartamento.
    /// </summary>
    /// <param name="ano">Ano de referencia.</param>
    /// <param name="mes">Mes de referencia (1-12).</param>
    [HttpGet("mensal/{ano:int}/{mes:int}")]
    [ProducesResponseType(typeof(RespostaApi<BalancoMensalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterBalancoMensal(int ano, int mes, CancellationToken cancellationToken)
    {
        if (mes < 1 || mes > 12)
            return BadRequest(RespostaErro.Criar("O mes deve estar entre 1 e 12."));

        var resultado = await _mediator.Send(new ObterBalancoMensalConsulta(ano, mes), cancellationToken);
        return Ok(RespostaApi<BalancoMensalDto>.Ok(resultado));
    }

    /// <summary>
    /// Retorna o balanco financeiro anual com resumo por mes.
    /// Permite visualizar a evolucao de receitas e gastos ao longo do ano.
    /// </summary>
    /// <param name="ano">Ano de referencia.</param>
    [HttpGet("anual/{ano:int}")]
    [ProducesResponseType(typeof(RespostaApi<BalancoAnualDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterBalancoAnual(int ano, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterBalancoAnualConsulta(ano), cancellationToken);
        return Ok(RespostaApi<BalancoAnualDto>.Ok(resultado));
    }
}

