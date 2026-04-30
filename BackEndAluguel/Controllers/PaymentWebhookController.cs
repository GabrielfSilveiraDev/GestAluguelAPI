using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsavel por receber os webhooks de pagamento do Asaas.
/// Este endpoint e chamado automaticamente pelo Asaas quando um pagamento PIX e confirmado.
///
/// Seguranca:
/// O Asaas envia um header "asaas-access-token" com um token configurado no painel.
/// Toda requisicao deve ser validada contra o token armazenado em appsettings.json
/// antes de processar qualquer dado. Requisicoes sem token valido sao rejeitadas com 401.
///
/// Pratica recomendada HTTP 200 rapido:
/// O endpoint retorna 200 OK o mais rapido possivel para evitar retentativas do Asaas.
/// O processamento e sincronizado mas minimo (apenas busca + atualizacao do status).
/// </summary>
[ApiController]
[Route("api/payment-webhook")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentWebhookController> _logger;

    /// <summary>Inicializa o controller com os servicos necessarios.</summary>
    public PaymentWebhookController(
        IFaturaRepositorio faturaRepositorio,
        IConfiguration configuration,
        ILogger<PaymentWebhookController> logger)
    {
        _faturaRepositorio = faturaRepositorio;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint que recebe notificacoes de pagamento do Asaas.
    /// Valida o token de autenticacao e processa o evento PAYMENT_RECEIVED ou PAYMENT_CONFIRMED.
    ///
    /// Logica do webhook:
    /// 1. Valida o token no header "asaas-access-token".
    /// 2. Verifica se o evento e de pagamento confirmado.
    /// 3. Busca a fatura pelo CobrancaAsaasId (id da cobranca no Asaas).
    /// 4. Registra o pagamento com a data atual se a fatura ainda nao estiver paga.
    /// 5. Retorna 200 OK imediatamente para evitar retentativas do Asaas.
    /// </summary>
    [HttpPost("asaas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReceberWebhookAsaas(
        [FromBody] AsaasWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        // Validacao do token de autenticacao enviado pelo Asaas
        var tokenEsperado = _configuration["Asaas:WebhookToken"];
        var tokenRecebido = Request.Headers["asaas-access-token"].FirstOrDefault();

        if (string.IsNullOrEmpty(tokenEsperado) || tokenRecebido != tokenEsperado)
        {
            _logger.LogWarning("Webhook Asaas recebido com token invalido. Token recebido: {Token}", tokenRecebido);
            return Unauthorized(new { mensagem = "Token de acesso invalido." });
        }

        // Eventos que indicam pagamento confirmado
        var eventosDeConfirmacao = new[] { "PAYMENT_RECEIVED", "PAYMENT_CONFIRMED" };

        if (!eventosDeConfirmacao.Contains(payload.Event, StringComparer.OrdinalIgnoreCase))
        {
            // Outros eventos (PAYMENT_CREATED, PAYMENT_OVERDUE, etc.) sao ignorados
            _logger.LogDebug("Webhook Asaas ignorado. Evento: {Evento}", payload.Event);
            return Ok(new { mensagem = "Evento ignorado." });
        }

        var cobrancaId = payload.Payment?.Id;
        if (string.IsNullOrEmpty(cobrancaId))
        {
            _logger.LogWarning("Webhook Asaas recebido sem ID de cobranca.");
            return Ok(new { mensagem = "ID de cobranca ausente." });
        }

        try
        {
            // Busca a fatura pelo ID externo do Asaas
            var fatura = await _faturaRepositorio.ObterPorCobrancaAsaasIdAsync(cobrancaId, cancellationToken);

            if (fatura is null)
            {
                _logger.LogWarning("Webhook Asaas: fatura nao encontrada para CobrancaId {CobrancaId}.", cobrancaId);
                // Retorna 200 mesmo assim para evitar retentativas desnecessarias
                return Ok(new { mensagem = "Fatura nao encontrada." });
            }

            if (fatura.Status == Domain.Enumeradores.StatusFatura.Pago)
            {
                _logger.LogInformation("Webhook Asaas: fatura {FaturaId} ja estava paga. Ignorando.", fatura.Id);
                return Ok(new { mensagem = "Fatura ja paga." });
            }

            // Registra o pagamento com a data de hoje
            var dataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            fatura.RegistrarPagamento(dataPagamento);
            _faturaRepositorio.Atualizar(fatura);
            await _faturaRepositorio.SalvarAlteracoesAsync(cancellationToken);

            _logger.LogInformation(
                "Webhook Asaas: fatura {FaturaId} marcada como paga via PIX. CobrancaId: {CobrancaId}.",
                fatura.Id, cobrancaId);
        }
        catch (Exception ex)
        {
            // Loga o erro mas retorna 200 para evitar retentativas infinitas do Asaas
            _logger.LogError(ex, "Erro ao processar webhook Asaas para CobrancaId {CobrancaId}.", cobrancaId);
        }

        // Retorna 200 OK o mais rapido possivel
        return Ok(new { mensagem = "Webhook processado com sucesso." });
    }
}

// ─── Modelos do payload do webhook Asaas ──────────────────────────────────────

/// <summary>Payload recebido pelo webhook do Asaas.</summary>
public sealed class AsaasWebhookPayload
{
    /// <summary>Tipo do evento (ex: "PAYMENT_RECEIVED", "PAYMENT_CONFIRMED").</summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>Dados do pagamento relacionado ao evento.</summary>
    [JsonPropertyName("payment")]
    public AsaasWebhookPagamento? Payment { get; set; }
}

/// <summary>Dados do pagamento dentro do payload do webhook.</summary>
public sealed class AsaasWebhookPagamento
{
    /// <summary>ID da cobranca no Asaas (ex: "pay_xxxxxxxxxxxxxxxx").</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Status atual do pagamento no Asaas.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Valor do pagamento.</summary>
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

