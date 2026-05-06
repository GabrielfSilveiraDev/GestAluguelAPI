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
/// Gerencia os valores de kWh, agua, WhatsApp e PIX utilizados no sistema.
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
    /// Retorna a configuracao global atual do sistema.
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
    /// </summary>
    [HttpPost("asaas/subconta")]
    [ProducesResponseType(typeof(RespostaApi<SubcontaResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarSubcontaAsaas([FromBody] CriarSubcontaAsaasComando comando, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(comando, cancellationToken);
        return Ok(RespostaApi<SubcontaResultadoDto>.Ok(resultado,
            $"Subconta Asaas criada com sucesso. WalletId '{resultado.WalletId}' salvo na configuracao."));
    }

    /// <summary>
    /// Configura o número de WhatsApp do locador e a mensagem padrão para envio de faturas.
    /// A mensagem suporta os placeholders: {inquilino}, {mesReferencia}, {valorTotal}, {dataVencimento}, {codigoPix}.
    /// O link gerado abre diretamente a conversa com o inquilino no WhatsApp com a mensagem pré-preenchida.
    /// Não requer nenhuma API externa — usa o link wa.me gratuito do WhatsApp.
    /// </summary>
    /// <param name="corpo">Número e template da mensagem.</param>
    /// <response code="200">Configuração WhatsApp salva com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPut("whatsapp")]
    [ProducesResponseType(typeof(RespostaApi<ConfiguracaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarWhatsapp([FromBody] AtualizarWhatsappCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new AtualizarWhatsappComando(corpo.NumeroWhatsapp, corpo.MensagemPadrao), cancellationToken);
        return Ok(RespostaApi<ConfiguracaoDto>.Ok(resultado, "Configuracao WhatsApp salva com sucesso."));
    }

    /// <summary>
    /// Configura os dados PIX nativos do locador para geração de código copia-e-cola sem gateway externo.
    /// Segue o padrão EMV do Banco Central do Brasil.
    /// Após configurar, use GET /api/faturas/{id}/pix-nativo para gerar o código de cada fatura.
    /// </summary>
    /// <param name="corpo">Chave PIX, nome e cidade do recebedor.</param>
    /// <response code="200">Configuração PIX salva com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPut("pix")]
    [ProducesResponseType(typeof(RespostaApi<ConfiguracaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarPix([FromBody] AtualizarPixCorpoConfig corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new AtualizarPixNativoComando(corpo.ChavePix, corpo.NomeRecebedor, corpo.CidadeRecebedor), cancellationToken);
        return Ok(RespostaApi<ConfiguracaoDto>.Ok(resultado, "Configuracao PIX salva com sucesso."));
    }
}

// ─── Corpos das requisições ──────────────────────────────────────────────────

/// <summary>Corpo da requisição de configuração do WhatsApp.</summary>
public record AtualizarWhatsappCorpo(
    /// <summary>
    /// Número do WhatsApp do locador no formato internacional sem '+' (ex: 5511999999999).
    /// Este será o número que receberá as respostas dos inquilinos.
    /// </summary>
    string NumeroWhatsapp,
    /// <summary>
    /// Template da mensagem padrão. Placeholders: {inquilino}, {mesReferencia}, {valorTotal}, {dataVencimento}, {codigoPix}.
    /// Exemplo: "Olá {inquilino}, segue sua fatura de {mesReferencia}.\nValor: R$ {valorTotal}\nVencimento: {dataVencimento}\nPIX:\n{codigoPix}"
    /// </summary>
    string MensagemPadrao
);

/// <summary>Corpo da requisição de configuração do PIX nativo.</summary>
public record AtualizarPixCorpoConfig(
    /// <summary>Chave PIX do locador (CPF, CNPJ, e-mail, +55telefone ou UUID aleatório).</summary>
    string ChavePix,
    /// <summary>Nome do recebedor conforme cadastro no banco (máx. 25 caracteres).</summary>
    string NomeRecebedor,
    /// <summary>Cidade do recebedor (máx. 15 caracteres).</summary>
    string CidadeRecebedor
);

