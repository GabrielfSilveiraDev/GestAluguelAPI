using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Faturas.Comandos;
using BackEndAluguel.Application.Faturas.Consultas;
using BackEndAluguel.Application.Faturas.DTOs;
using BackEndAluguel.Application.Pagamentos.DTOs;
using BackEndAluguel.Domain.Enumeradores;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de gerenciamento de faturas mensais.
/// Delega toda a lógica aos manipuladores CQRS via MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FaturasController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Inicializa o controller com o mediator injetado.
    /// </summary>
    public FaturasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Busca uma fatura específica pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único (Guid) da fatura.</param>
    /// <response code="200">Fatura encontrada.</response>
    /// <response code="404">Fatura não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<FaturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterFaturaPorIdConsulta(id), cancellationToken);

        if (resultado is null)
            return NotFound(RespostaErro.Criar($"Fatura com id '{id}' não encontrada."));

        return Ok(RespostaApi<FaturaDto>.Ok(resultado));
    }

    /// <summary>
    /// Retorna todas as faturas de um inquilino específico.
    /// </summary>
    /// <param name="inquilinoId">Identificador do inquilino.</param>
    /// <response code="200">Lista de faturas do inquilino.</response>
    [HttpGet("inquilino/{inquilinoId:guid}")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<FaturaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorInquilino(Guid inquilinoId, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarFaturasPorInquilinoConsulta(inquilinoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<FaturaDto>>.Ok(resultado));
    }

    /// <summary>
    /// Retorna todas as faturas filtradas por status.
    /// Status: 1 = Pendente, 2 = Atrasado, 3 = Pago.
    /// </summary>
    /// <param name="status">Status da fatura (1, 2 ou 3).</param>
    /// <response code="200">Lista de faturas com o status informado.</response>
    /// <response code="400">Status inválido.</response>
    [HttpGet("status/{status:int}")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<FaturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorStatus(int status, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(StatusFatura), status))
            return BadRequest(RespostaErro.Criar($"Status '{status}' inválido. Use: 1=Pendente, 2=Atrasado, 3=Pago."));

        var resultado = await _mediator.Send(new ListarFaturasPorStatusConsulta((StatusFatura)status), cancellationToken);
        return Ok(RespostaApi<IEnumerable<FaturaDto>>.Ok(resultado));
    }

    /// <summary>
    /// Retorna todas as faturas vencidas (data limite ultrapassada e não pagas).
    /// </summary>
    /// <response code="200">Lista de faturas vencidas.</response>
    [HttpGet("vencidas")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<FaturaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterVencidas(CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarFaturasVencidasConsulta(), cancellationToken);
        return Ok(RespostaApi<IEnumerable<FaturaDto>>.Ok(resultado));
    }

    /// <summary>
    /// Gera uma nova fatura mensal para um inquilino.
    /// Não é permitido gerar mais de uma fatura por mês de referência para o mesmo inquilino.
    /// </summary>
    /// <param name="comando">Dados da fatura a ser gerada.</param>
    /// <response code="201">Fatura gerada com sucesso.</response>
    /// <response code="400">Fatura duplicada no mês, inquilino não encontrado ou dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RespostaApi<FaturaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarFaturaComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id },
            RespostaApi<FaturaDto>.Ok(resultado, "Fatura gerada com sucesso."));
    }

    /// <summary>
    /// Registra o pagamento de uma fatura, alterando o status para Pago.
    /// Não é possível registrar pagamento em uma fatura já paga.
    /// </summary>
    /// <param name="id">Identificador único da fatura.</param>
    /// <param name="corpo">Data do pagamento realizado.</param>
    /// <response code="200">Pagamento registrado com sucesso.</response>
    /// <response code="404">Fatura não encontrada.</response>
    /// <response code="422">Fatura já está paga.</response>
    [HttpPost("{id:guid}/pagamento")]
    [ProducesResponseType(typeof(RespostaApi<FaturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarPagamento(Guid id, [FromBody] RegistrarPagamentoCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new RegistrarPagamentoComando(id, corpo.DataPagamento), cancellationToken);
        return Ok(RespostaApi<FaturaDto>.Ok(resultado, "Pagamento registrado com sucesso."));
    }

    /// <summary>
    /// Atualiza a leitura de kWh atual da fatura e recalcula automaticamente o valor da luz.
    /// Fórmula: ValorLuz = (KwAtual - KwMesAnterior) × KwhValor (da configuração ou do override).
    /// </summary>
    /// <param name="id">Identificador único da fatura.</param>
    /// <param name="corpo">Nova leitura em kWh e valor opcional do kWh.</param>
    /// <response code="200">Leitura e valor da luz atualizados com sucesso.</response>
    /// <response code="404">Fatura não encontrada.</response>
    /// <response code="422">Fatura já está paga.</response>
    [HttpPatch("{id:guid}/leitura-kw")]
    [ProducesResponseType(typeof(RespostaApi<FaturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarLeituraKw(Guid id, [FromBody] AtualizarLeituraKwCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new AtualizarLeituraKwComando(id, corpo.KwAtual, corpo.KwhValorOverride), cancellationToken);
        return Ok(RespostaApi<FaturaDto>.Ok(resultado, "Leitura de kWh atualizada e valor da luz recalculado com sucesso."));
    }

    /// <summary>
    /// Atualiza os valores de água e luz de uma fatura ainda não paga.
    /// </summary>
    /// <param name="id">Identificador único da fatura.</param>
    /// <param name="corpo">Novos valores de água e luz.</param>
    /// <response code="200">Valores atualizados com sucesso.</response>
    /// <response code="404">Fatura não encontrada.</response>
    /// <response code="422">Fatura já está paga.</response>
    [HttpPatch("{id:guid}/consumo")]
    [ProducesResponseType(typeof(RespostaApi<FaturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarConsumo(Guid id, [FromBody] AtualizarConsumoCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new AtualizarValoresConsumoComando(id, corpo.ValorAgua, corpo.ValorLuz), cancellationToken);
        return Ok(RespostaApi<FaturaDto>.Ok(resultado, "Valores de consumo atualizados com sucesso."));
    }

    /// <summary>
    /// Atualiza o código ou link do PIX de uma fatura.
    /// </summary>
    /// <param name="id">Identificador único da fatura.</param>
    /// <param name="corpo">Novo código PIX.</param>
    /// <response code="200">Código PIX atualizado com sucesso.</response>
    /// <response code="404">Fatura não encontrada.</response>
    [HttpPatch("{id:guid}/pix")]
    [ProducesResponseType(typeof(RespostaApi<FaturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarPix(Guid id, [FromBody] AtualizarPixCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new AtualizarCodigoPixComando(id, corpo.CodigoPix), cancellationToken);
        return Ok(RespostaApi<FaturaDto>.Ok(resultado, "Código PIX atualizado com sucesso."));
    }


    /// <summary>
    /// Gera uma cobrança PIX via gateway Asaas para a fatura especificada.
    /// O resultado inclui o código PIX copia-e-cola e a URL do QR Code para exibição ao inquilino.
    /// O CobrancaAsaasId e o CodigoPix são automaticamente salvos na fatura.
    /// Pré-requisito: o WalletId do locador deve estar configurado (POST /api/configuracoes/asaas/subconta).
    /// </summary>
    /// <param name="id">Identificador único da fatura.</param>
    /// <param name="corpo">Percentual de split (opcional).</param>
    /// <response code="200">Cobrança PIX gerada. Retorna código copia-e-cola e QR Code.</response>
    /// <response code="404">Fatura não encontrada.</response>
    /// <response code="422">Fatura já paga ou WalletId não configurado.</response>
    [HttpPost("{id:guid}/gerar-pix")]
    [ProducesResponseType(typeof(RespostaApi<CobrancaPixResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GerarPix(Guid id, [FromBody] GerarPixCorpo? corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new GerarCobrancaPixComando(id, corpo?.PercentualSplit), cancellationToken);
        return Ok(RespostaApi<CobrancaPixResultadoDto>.Ok(resultado, "Cobrança PIX gerada com sucesso."));
    }
}

// ─── Corpos das requisições ──────────────────────────────────────────────────

/// <summary>
/// Corpo da requisição de registro de pagamento.
/// </summary>
public record RegistrarPagamentoCorpo(
    /// <summary>Data em que o pagamento foi efetuado.</summary>
    DateOnly DataPagamento
);

/// <summary>
/// Corpo da requisição de atualização dos valores de consumo (água e luz).
/// </summary>
public record AtualizarConsumoCorpo(
    /// <summary>Novo valor da conta de água.</summary>
    decimal ValorAgua,
    /// <summary>Novo valor da conta de luz.</summary>
    decimal ValorLuz
);

/// <summary>
/// Corpo da requisição de atualização do código PIX.
/// </summary>
public record AtualizarPixCorpo(
    /// <summary>Novo código ou link do PIX.</summary>
    string CodigoPix
);

/// <summary>
/// Corpo da requisição de atualização da leitura de kWh.
/// </summary>
public record AtualizarLeituraKwCorpo(
    /// <summary>Nova leitura do relógio em kWh.</summary>
    decimal KwAtual,
    /// <summary>Valor do kWh em R$ (opcional — usa configuração global se não informado).</summary>
    decimal? KwhValorOverride = null
);

/// <summary>
/// Corpo da requisição de geração de cobrança PIX via Asaas.
/// </summary>
public record GerarPixCorpo(
    /// <summary>
    /// Percentual do valor a ser direcionado ao WalletId do host (0-100).
    /// Se nulo, o valor total é direcionado ao host.
    /// </summary>
    decimal? PercentualSplit = null
);

