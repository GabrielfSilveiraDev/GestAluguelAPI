using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Apartamentos.Comandos;
using BackEndAluguel.Application.Apartamentos.Consultas;
using BackEndAluguel.Application.Apartamentos.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de gerenciamento de apartamentos.
/// Delega toda a lógica aos manipuladores CQRS via MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApartamentosController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Inicializa o controller com o mediator injetado.
    /// </summary>
    public ApartamentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna a lista de todos os apartamentos cadastrados.
    /// </summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<ApartamentoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarApartamentosConsulta(), cancellationToken);
        return Ok(RespostaApi<IEnumerable<ApartamentoDto>>.Ok(resultado));
    }

    /// <summary>
    /// Retorna apenas os apartamentos disponíveis (desocupados).
    /// Útil para popular o dropdown de seleção no cadastro de inquilino.
    /// </summary>
    /// <response code="200">Lista de apartamentos disponíveis.</response>
    [HttpGet("disponiveis")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<ApartamentoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterDisponiveis(CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarApartamentosDesocupadosConsulta(), cancellationToken);
        return Ok(RespostaApi<IEnumerable<ApartamentoDto>>.Ok(resultado));
    }

    /// <summary>
    /// Busca um apartamento específico pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único (Guid) do apartamento.</param>
    /// <response code="200">Apartamento encontrado.</response>
    /// <response code="404">Apartamento não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<ApartamentoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterApartamentoPorIdConsulta(id), cancellationToken);

        if (resultado is null)
            return NotFound(RespostaErro.Criar($"Apartamento com id '{id}' não encontrado."));

        return Ok(RespostaApi<ApartamentoDto>.Ok(resultado));
    }

    /// <summary>
    /// Cadastra um novo apartamento no sistema.
    /// O número e bloco devem ser únicos — não podem existir dois apartamentos iguais.
    /// </summary>
    /// <param name="comando">Dados do apartamento a ser criado.</param>
    /// <response code="201">Apartamento criado com sucesso.</response>
    /// <response code="400">Dados inválidos ou apartamento duplicado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RespostaApi<ApartamentoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarApartamentoComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id },
            RespostaApi<ApartamentoDto>.Ok(resultado, "Apartamento criado com sucesso."));
    }

    /// <summary>
    /// Atualiza os dados de um apartamento existente.
    /// </summary>
    /// <param name="id">Identificador único do apartamento.</param>
    /// <param name="corpo">Novos dados do apartamento.</param>
    /// <response code="200">Apartamento atualizado com sucesso.</response>
    /// <response code="404">Apartamento não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<ApartamentoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarApartamentoCorpo corpo, CancellationToken cancellationToken)
    {
        var comando = new AtualizarApartamentoComando(id, corpo.Numero, corpo.Bloco);
        var resultado = await _mediator.Send(comando, cancellationToken);
        return Ok(RespostaApi<ApartamentoDto>.Ok(resultado, "Apartamento atualizado com sucesso."));
    }

    /// <summary>
    /// Remove um apartamento do sistema.
    /// Não é possível remover um apartamento que esteja ocupado por um inquilino.
    /// </summary>
    /// <param name="id">Identificador único do apartamento a ser removido.</param>
    /// <response code="204">Apartamento removido com sucesso.</response>
    /// <response code="400">Não é possível remover — apartamento ocupado.</response>
    /// <response code="404">Apartamento não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoverApartamentoComando(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Corpo da requisição de atualização de apartamento.
/// Separa o Id (vindo pela rota) dos dados do corpo da requisição.
/// </summary>
public record AtualizarApartamentoCorpo(
    /// <summary>Novo número do apartamento.</summary>
    string Numero,
    /// <summary>Novo bloco do apartamento.</summary>
    string Bloco
);

