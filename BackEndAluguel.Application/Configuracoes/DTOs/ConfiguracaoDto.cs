namespace BackEndAluguel.Application.Configuracoes.DTOs;

/// <summary>
/// DTO de leitura da configuracao global do sistema.
/// </summary>
public record ConfiguracaoDto(
    /// <summary>Identificador unico da configuracao.</summary>
    Guid Id,
    /// <summary>Valor do kWh em reais (usado para calcular o custo de energia).</summary>
    decimal KwhValor,
    /// <summary>Valor fixo mensal da agua cobrado de todos os inquilinos.</summary>
    decimal ValorAgua,
    /// <summary>Data da ultima atualizacao da configuracao.</summary>
    DateTime? AtualizadoEm,
    /// <summary>WalletId do Host na plataforma Asaas.</summary>
    string? WalletIdAsaas = null,
    /// <summary>Número de WhatsApp do locador no formato internacional (ex: 5511999999999).</summary>
    string? NumeroWhatsappLocador = null,
    /// <summary>Template da mensagem padrão WhatsApp. Placeholders: {inquilino}, {mesReferencia}, {valorTotal}, {dataVencimento}, {codigoPix}.</summary>
    string? MensagemPadraoWhatsapp = null,
    /// <summary>Chave PIX do locador (para geração de código nativo sem gateway).</summary>
    string? ChavePix = null,
    /// <summary>Nome do recebedor PIX (máx. 25 chars).</summary>
    string? NomeRecebedorPix = null,
    /// <summary>Cidade do recebedor PIX (máx. 15 chars).</summary>
    string? CidadeRecebedorPix = null
);

/// <summary>
/// DTO de resultado do link WhatsApp gerado para uma fatura.
/// </summary>
public record WhatsAppLinkDto(
    /// <summary>URL wa.me com o número e mensagem pré-preenchida. Abrir no frontend.</summary>
    string LinkWhatsApp,
    /// <summary>Mensagem formatada que será enviada.</summary>
    string Mensagem,
    /// <summary>Código PIX copia-e-cola gerado para inclusão na mensagem (nulo se ChavePix não configurada).</summary>
    string? CodigoPix,
    /// <summary>Número de telefone do inquilino (para referência).</summary>
    string TelefoneInquilino
);

