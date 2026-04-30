namespace BackEndAluguel.Application.Pagamentos.DTOs;

/// <summary>
/// Dados necessarios para registrar uma subconta (Host) na API do Asaas.
/// </summary>
public record CriarSubcontaDto(
    /// <summary>Nome completo ou razao social do Host.</summary>
    string Nome,
    /// <summary>E-mail do Host para acesso a subconta Asaas.</summary>
    string Email,
    /// <summary>CPF ou CNPJ do Host (somente digitos).</summary>
    string CpfCnpj,
    /// <summary>Tipo de pessoa: "FISICA" ou "JURIDICA".</summary>
    string TipoPessoa,
    /// <summary>Telefone de contato do Host.</summary>
    string? Telefone = null,
    /// <summary>Site do Host (opcional).</summary>
    string? Site = null
);

/// <summary>
/// Resultado do registro de subconta no Asaas.
/// Contem o walletId necessario para configurar o split de pagamento.
/// </summary>
public record SubcontaResultadoDto(
    /// <summary>ID interno da conta gerado pelo Asaas.</summary>
    string Id,
    /// <summary>
    /// Identificador da carteira (walletId) do Host.
    /// Este valor deve ser armazenado para uso no split de pagamentos PIX.
    /// </summary>
    string WalletId,
    /// <summary>Nome da conta cadastrada.</summary>
    string Nome,
    /// <summary>E-mail da conta cadastrada.</summary>
    string Email,
    /// <summary>Status da conta no Asaas (ex: "ACTIVE", "PENDING").</summary>
    string Status
);

/// <summary>
/// Dados necessarios para gerar uma cobranca PIX com split de pagamento.
/// </summary>
public record CriarCobrancaPixDto(
    /// <summary>Identificador da fatura no nosso sistema.</summary>
    Guid FaturaId,
    /// <summary>CPF do inquilino (pagador).</summary>
    string CpfInquilino,
    /// <summary>Nome do inquilino (pagador).</summary>
    string NomeInquilino,
    /// <summary>Valor total da cobranca em reais.</summary>
    decimal Valor,
    /// <summary>Descricao da cobranca que aparece para o pagador.</summary>
    string Descricao,
    /// <summary>
    /// WalletId do Host para onde o valor sera direcionado via split.
    /// Obtido no cadastro da subconta.
    /// </summary>
    string WalletIdHost,
    /// <summary>
    /// Percentual do valor a ser transferido ao Host (0-100).
    /// Se nulo, o valor total e transferido.
    /// </summary>
    decimal? PercentualSplit = null
);

/// <summary>
/// Resultado da geracao de cobranca PIX pelo Asaas.
/// Contem os dados necessarios para o pagamento pelo inquilino.
/// </summary>
public record CobrancaPixResultadoDto(
    /// <summary>ID da cobranca no Asaas (ex: "pay_xxxx"). Usado para controle via webhook.</summary>
    string CobrancaId,
    /// <summary>
    /// Codigo PIX copia-e-cola (payload completo).
    /// Deve ser exibido ao inquilino para que possa copiar e colar no app bancario.
    /// </summary>
    string? PixCopiaCola,
    /// <summary>URL ou base64 do QR Code gerado para o pagamento.</summary>
    string? QrCodeUrl,
    /// <summary>Status da cobranca (ex: "PENDING").</summary>
    string Status,
    /// <summary>Valor da cobranca.</summary>
    decimal Valor
);

