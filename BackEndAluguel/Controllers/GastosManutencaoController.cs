using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.GastosManutencao.Comandos;
using BackEndAluguel.Application.GastosManutencao.Consultas;
using BackEndAluguel.Application.GastosManutencao.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsavel pelos endpoints de gastos de manutencao dos apartamentos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GastosManutencaoController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Inicializa o controller com o mediator injetado.</summary>
    public GastosManutencaoController(IMediator mediator) { _mediator = mediator; }

    /// <summary>Busca um gasto pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<GastoManutencaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterGastoManutencaoPorIdConsulta(id), cancellationToken);
        if (resultado is null)
            return NotFound(RespostaErro.Criar($"Gasto com id '{id}' nao encontrado."));
        return Ok(RespostaApi<GastoManutencaoDto>.Ok(resultado));
    }

    /// <summary>Lista todos os gastos de um apartamento.</summary>
    [HttpGet("apartamento/{apartamentoId:guid}")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<GastoManutencaoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorApartamento(Guid apartamentoId, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarGastosPorApartamentoConsulta(apartamentoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<GastoManutencaoDto>>.Ok(resultado));
    }

    /// <summary>Lista os gastos de manutencao de um mes/ano especifico.</summary>
    [HttpGet("mes/{ano:int}/{mes:int}")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<GastoManutencaoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorMes(int ano, int mes, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarGastosPorMesConsulta(ano, mes), cancellationToken);
        return Ok(RespostaApi<IEnumerable<GastoManutencaoDto>>.Ok(resultado));
    }

    /// <summary>Lista todos os gastos de manutencao de um ano.</summary>
    [HttpGet("ano/{ano:int}")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<GastoManutencaoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorAno(int ano, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarGastosPorAnoConsulta(ano), cancellationToken);
        return Ok(RespostaApi<IEnumerable<GastoManutencaoDto>>.Ok(resultado));
    }

    /// <summary>Registra um novo gasto de manutencao em um apartamento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RespostaApi<GastoManutencaoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarGastoManutencaoComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id },
            RespostaApi<GastoManutencaoDto>.Ok(resultado, "Gasto de manutencao registrado com sucesso."));
    }

    /// <summary>Atualiza um gasto de manutencao existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<GastoManutencaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarGastoCorpo corpo, CancellationToken cancellationToken)
    {
        var comando = new AtualizarGastoManutencaoComando(id, corpo.Descricao, corpo.Valor, corpo.Data, corpo.Observacao);
        var resultado = await _mediator.Send(comando, cancellationToken);
        return Ok(RespostaApi<GastoManutencaoDto>.Ok(resultado, "Gasto atualizado com sucesso."));
    }

    /// <summary>Remove um gasto de manutencao.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoverGastoManutencaoComando(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Corpo da requisicao de atualizacao de gasto de manutencao.</summary>
public record AtualizarGastoCorpo(string Descricao, decimal Valor, DateOnly Data, string? Observacao = null);

