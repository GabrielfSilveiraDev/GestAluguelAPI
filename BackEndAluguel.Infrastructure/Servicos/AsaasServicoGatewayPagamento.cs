using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BackEndAluguel.Application.Pagamentos;
using BackEndAluguel.Application.Pagamentos.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementacao do servico de gateway de pagamento utilizando a API do Asaas.
/// Responsavel por registrar subcontas (Hosts) e gerar cobranças PIX com split de pagamento.
///
/// Logica de requisicao:
/// 1. O IHttpClientFactory fornece um HttpClient ja configurado com a URL base e o header
///    de autenticacao "$asaas_api_key", evitando duplicacao de configuracao.
/// 2. Os payloads sao serializados em JSON seguindo a nomenclatura esperada pelo Asaas.
/// 3. Em caso de erro HTTP, o corpo da resposta e lido e logado para facilitar o diagnostico.
/// 4. O split de pagamento funciona enviando o array "split" no payload da cobranca,
///    contendo o walletId do Host e o percentual ou valor fixo a ser transferido.
/// </summary>
public class AsaasServicoGatewayPagamento : IServicoGatewayPagamento
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AsaasServicoGatewayPagamento> _logger;

    // Nome do cliente HTTP registrado no DI container (ver InfrastructureExtensoes)
    private const string NomeClienteHttp = "Asaas";

    /// <summary>
    /// Inicializa o servico com a fabrica de clientes HTTP e o logger.
    /// </summary>
    /// <param name="httpClientFactory">Fabrica de clientes HTTP (IHttpClientFactory).</param>
    /// <param name="logger">Logger para registro de operacoes e erros.</param>
    public AsaasServicoGatewayPagamento(
        IHttpClientFactory httpClientFactory,
        ILogger<AsaasServicoGatewayPagamento> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Registra uma nova subconta no Asaas para um Host (locador).
    ///
    /// Fluxo da requisicao:
    /// 1. Monta o payload com os dados do Host.
    /// 2. Envia POST para /accounts na API do Asaas.
    /// 3. Extrai o walletId e demais informacoes da resposta.
    /// O walletId retornado deve ser armazenado pelo chamador para uso futuro no split de pagamentos.
    /// </summary>
    public async Task<SubcontaResultadoDto> CriarSubcontaAsync(
        CriarSubcontaDto dados,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando registro de subconta Asaas para: {Nome}", dados.Nome);

        var cliente = _httpClientFactory.CreateClient(NomeClienteHttp);

        // Payload conforme documentacao da API Asaas v3
        var payload = new
        {
            name = dados.Nome,
            email = dados.Email,
            cpfCnpj = dados.CpfCnpj,
            companyType = dados.TipoPessoa == "JURIDICA" ? "LIMITED" : (string?)null,
            phone = dados.Telefone,
            site = dados.Site
        };

        var resposta = await cliente.PostAsJsonAsync("/accounts", payload, cancellationToken);

        if (!resposta.IsSuccessStatusCode)
        {
            var corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao criar subconta Asaas. Status: {Status}. Corpo: {Corpo}",
                resposta.StatusCode, corpo);
            throw new InvalidOperationException(
                $"Erro ao registrar subconta no Asaas. Status: {resposta.StatusCode}. Detalhes: {corpo}");
        }

        // Desserializa a resposta da API do Asaas
        var json = await resposta.Content.ReadFromJsonAsync<AsaasContaResposta>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta vazia da API Asaas ao criar subconta.");

        _logger.LogInformation("Subconta Asaas criada com sucesso. WalletId: {WalletId}", json.WalletId);

        return new SubcontaResultadoDto(
            Id: json.Id ?? string.Empty,
            WalletId: json.WalletId ?? string.Empty,
            Nome: json.Name ?? string.Empty,
            Email: json.Email ?? string.Empty,
            Status: json.Status ?? string.Empty
        );
    }

    /// <summary>
    /// Gera uma cobranca PIX com configuracao de split de pagamento para o walletId do Host.
    ///
    /// Fluxo da requisicao:
    /// 1. Monta o payload com os dados da cobranca, destinatario (Customer) e configuracao de split.
    /// 2. Envia POST para /payments com billingType=PIX.
    ///    O campo "split" informa ao Asaas para transferir automaticamente o valor/percentual
    ///    para o walletId do Host apos a confirmacao do pagamento.
    /// 3. Apos criar a cobranca, consulta GET /payments/{id}/pixQrCode para obter o copia-e-cola.
    /// 4. Retorna o cobrancaId (para armazenar e rastrear via webhook), o copia-e-cola e o QR Code.
    /// </summary>
    public async Task<CobrancaPixResultadoDto> CriarCobrancaPixAsync(
        CriarCobrancaPixDto dados,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Gerando cobranca PIX para fatura {FaturaId}. Valor: {Valor}",
            dados.FaturaId, dados.Valor);

        var cliente = _httpClientFactory.CreateClient(NomeClienteHttp);

        // Configuracao do split: direciona o valor para o walletId do Host
        // Se PercentualSplit informado, usa percentual; caso contrario envia o valor fixo total.
        var splitConfig = dados.PercentualSplit.HasValue
            ? new object[] { new { walletId = dados.WalletIdHost, percentualValor = dados.PercentualSplit.Value } }
            : new object[] { new { walletId = dados.WalletIdHost, fixedValue = dados.Valor } };

        // Payload da cobranca PIX conforme documentacao Asaas v3
        var payload = new
        {
            billingType = "PIX",
            // Referencia interna para rastreabilidade (nosso ID de fatura)
            externalReference = dados.FaturaId.ToString(),
            customer = dados.CpfInquilino, // CPF do pagador (o Asaas aceita CPF diretamente)
            value = dados.Valor,
            description = dados.Descricao,
            // Split de pagamento: valor vai direto para a carteira do Host
            split = splitConfig
        };

        var resposta = await cliente.PostAsJsonAsync("/payments", payload, cancellationToken);

        if (!resposta.IsSuccessStatusCode)
        {
            var corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao criar cobranca PIX Asaas. Status: {Status}. Corpo: {Corpo}",
                resposta.StatusCode, corpo);
            throw new InvalidOperationException(
                $"Erro ao gerar cobranca PIX no Asaas. Status: {resposta.StatusCode}. Detalhes: {corpo}");
        }

        var pagamento = await resposta.Content.ReadFromJsonAsync<AsaasPagamentoResposta>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta vazia da API Asaas ao criar cobranca PIX.");

        _logger.LogInformation("Cobranca PIX criada com sucesso. CobrancaId: {CobrancaId}", pagamento.Id);

        // Consulta o QR Code e copia-e-cola do PIX
        string? pixCopiaCola = null;
        string? qrCodeUrl = null;

        if (!string.IsNullOrEmpty(pagamento.Id))
        {
            var qrResp = await cliente.GetAsync($"/payments/{pagamento.Id}/pixQrCode", cancellationToken);
            if (qrResp.IsSuccessStatusCode)
            {
                var qr = await qrResp.Content.ReadFromJsonAsync<AsaasPixQrCode>(
                    cancellationToken: cancellationToken);
                pixCopiaCola = qr?.Payload;
                qrCodeUrl = qr?.EncodedImage;
            }
            else
            {
                _logger.LogWarning("Nao foi possivel obter o QR Code PIX para cobranca {CobrancaId}", pagamento.Id);
            }
        }

        return new CobrancaPixResultadoDto(
            CobrancaId: pagamento.Id ?? string.Empty,
            PixCopiaCola: pixCopiaCola,
            QrCodeUrl: qrCodeUrl,
            Status: pagamento.Status ?? "PENDING",
            Valor: dados.Valor
        );
    }

    // ─────────────────────────────────────────────────────────────────
    // Modelos internos para desserializar as respostas da API do Asaas
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Modelo de resposta da criacao de conta no Asaas.</summary>
    private sealed class AsaasContaResposta
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("walletId")]
        public string? WalletId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    /// <summary>Modelo de resposta da criacao de pagamento no Asaas.</summary>
    private sealed class AsaasPagamentoResposta
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    /// <summary>Modelo de resposta do endpoint de QR Code PIX do Asaas.</summary>
    private sealed class AsaasPixQrCode
    {
        /// <summary>Codigo PIX copia-e-cola.</summary>
        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        /// <summary>Imagem do QR Code em base64.</summary>
        [JsonPropertyName("encodedImage")]
        public string? EncodedImage { get; set; }
    }
}

