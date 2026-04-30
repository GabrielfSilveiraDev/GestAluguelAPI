using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Inquilinos.Comandos;
using BackEndAluguel.Application.Inquilinos.Consultas;
using BackEndAluguel.Application.Inquilinos.DTOs;
using BackEndAluguel.Domain.Enumeradores;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BackEndAluguel.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InquilinosController : ControllerBase
{
    private readonly IMediator _mediator;
    public InquilinosController(IMediator mediator) { _mediator = mediator; }
    [HttpGet]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<InquilinoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarInquilinosConsulta(), cancellationToken);
        return Ok(RespostaApi<IEnumerable<InquilinoDto>>.Ok(resultado));
    }
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<InquilinoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterInquilinoPorIdConsulta(id), cancellationToken);
        if (resultado is null) return NotFound(RespostaErro.Criar($"Inquilino '{id}' nao encontrado."));
        return Ok(RespostaApi<InquilinoDto>.Ok(resultado));
    }
    [HttpGet("apartamento/{apartamentoId:guid}")]
    public async Task<IActionResult> ObterPorApartamento(Guid apartamentoId, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarInquilinosPorApartamentoConsulta(apartamentoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<InquilinoDto>>.Ok(resultado));
    }
    [HttpGet("contrato-proximo")]
    public async Task<IActionResult> ObterComContratoProximo([FromQuery] int dias = 30, CancellationToken cancellationToken = default)
    {
        var resultado = await _mediator.Send(new ListarInquilinosComContratoProximoConsulta(dias), cancellationToken);
        return Ok(RespostaApi<IEnumerable<InquilinoDto>>.Ok(resultado));
    }
    [HttpPost]
    [ProducesResponseType(typeof(RespostaApi<InquilinoDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] CriarInquilinoComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, RespostaApi<InquilinoDto>.Ok(resultado, "Inquilino cadastrado com sucesso."));
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarInquilinoCorpo corpo, CancellationToken cancellationToken)
    {
        var cmd = new AtualizarInquilinoComando(id, corpo.NomeCompleto, corpo.QuantidadeMoradores, corpo.DataVencimentoContrato, corpo.ValorAluguel, corpo.Rg, corpo.OrgaoEmissor, corpo.Telefone, corpo.EstadoCivil, corpo.Garagem, corpo.DiasAlertaVencimento);
        var resultado = await _mediator.Send(cmd, cancellationToken);
        return Ok(RespostaApi<InquilinoDto>.Ok(resultado, "Inquilino atualizado com sucesso."));
    }
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoverInquilinoComando(id), cancellationToken);
        return NoContent();
    }
    [HttpGet("{inquilinoId:guid}/dependentes")]
    public async Task<IActionResult> ListarDependentes(Guid inquilinoId, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarDependentesPorInquilinoConsulta(inquilinoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<DependenteDto>>.Ok(resultado));
    }
    [HttpGet("dependentes/{id:guid}")]
    public async Task<IActionResult> ObterDependentePorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterDependentePorIdConsulta(id), cancellationToken);
        if (resultado is null) return NotFound(RespostaErro.Criar($"Dependente '{id}' nao encontrado."));
        return Ok(RespostaApi<DependenteDto>.Ok(resultado));
    }
    [HttpPost("{inquilinoId:guid}/dependentes")]
    [ProducesResponseType(typeof(RespostaApi<DependenteDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AdicionarDependente(Guid inquilinoId, [FromBody] AdicionarDependenteCorpo corpo, CancellationToken cancellationToken)
    {
        var cmd = new AdicionarDependenteComando(inquilinoId, corpo.NomeCompleto, corpo.Cpf, corpo.Rg, corpo.OrgaoEmissor, corpo.DataNascimento, corpo.Telefone, corpo.EstadoCivil);
        var resultado = await _mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(ObterDependentePorId), new { id = resultado.Id }, RespostaApi<DependenteDto>.Ok(resultado, "Dependente adicionado com sucesso."));
    }
    [HttpPut("dependentes/{id:guid}")]
    public async Task<IActionResult> AtualizarDependente(Guid id, [FromBody] AtualizarDependenteCorpo corpo, CancellationToken cancellationToken)
    {
        var cmd = new AtualizarDependenteComando(id, corpo.NomeCompleto, corpo.Rg, corpo.OrgaoEmissor, corpo.DataNascimento, corpo.Telefone, corpo.EstadoCivil);
        var resultado = await _mediator.Send(cmd, cancellationToken);
        return Ok(RespostaApi<DependenteDto>.Ok(resultado, "Dependente atualizado com sucesso."));
    }
    [HttpDelete("dependentes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoverDependente(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoverDependenteComando(id), cancellationToken);
        return NoContent();
    }
}
public record AtualizarInquilinoCorpo(string NomeCompleto, int QuantidadeMoradores, DateOnly DataVencimentoContrato, decimal ValorAluguel, string Rg, string OrgaoEmissor, string Telefone, EstadoCivil EstadoCivil, decimal Garagem = 0m, List<int>? DiasAlertaVencimento = null);
public record AdicionarDependenteCorpo(string NomeCompleto, string Cpf, string Rg, string OrgaoEmissor, DateOnly DataNascimento, string Telefone, EstadoCivil EstadoCivil);
public record AtualizarDependenteCorpo(string NomeCompleto, string Rg, string OrgaoEmissor, DateOnly DataNascimento, string Telefone, EstadoCivil EstadoCivil);