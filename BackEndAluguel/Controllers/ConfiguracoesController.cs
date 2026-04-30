using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Configuracoes.Comandos;
using BackEndAluguel.Application.Configuracoes.Consultas;
using BackEndAluguel.Application.Configuracoes.DTOs;
using BackEndAluguel.Application.Pagamentos.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsavel pelos endpoints de configuracao global do sistema.
/// Gerencia os valores de kWh e agua utilizados no calculo automatico das faturas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConfiguracoesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Inicializa o controller com o mediator injetado.</summary>
    public ConfiguracoesController(IMediator mediator) { _mediator = mediator; }

    /// <summary>
    /// Retorna a configuracao global atual do sistema (valor do kWh e valor da agua).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(RespostaApi<ConfiguracaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterConfiguracaoConsulta(), cancellationToken);
        return Ok(RespostaApi<ConfiguracaoDto>.Ok(resultado));
    }

    /// <summary>
    /// Atualiza os valores de kWh e agua da configuracao global.
    /// Esses valores sao usados automaticamente em novas faturas.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(RespostaApi<ConfiguracaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarConfiguracaoComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return Ok(RespostaApi<ConfiguracaoDto>.Ok(resultado, "Configuracao atualizada com sucesso."));
    }

    /// <summary>
    /// Registra o locador (host) na plataforma Asaas como subconta.
    /// O WalletId retornado e salvo automaticamente na configuracao global
    /// e sera usado no split de pagamentos PIX gerados para as faturas.
    /// Este endpoint deve ser chamado uma unica vez na configuracao inicial do sistema.
    /// </summary>
    /// <param name="comando">Dados do host para cadastro na Asaas.</param>
    /// <response code="200">Subconta criada com sucesso. WalletId salvo na configuracao.</response>
    /// <response code="400">Dados invalidos ou erro ao comunicar com a API Asaas.</response>
    [HttpPost("asaas/subconta")]
    [ProducesResponseType(typeof(RespostaApi<SubcontaResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarSubcontaAsaas([FromBody] CriarSubcontaAsaasComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return Ok(RespostaApi<SubcontaResultadoDto>.Ok(resultado,
            $"Subconta Asaas criada com sucesso. WalletId '{resultado.WalletId}' salvo na configuracao."));
    }
}

