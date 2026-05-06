namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Configuracao global do sistema (singleton).
/// Armazena o valor do kWh e o valor fixo da agua usados na geracao de faturas.
/// </summary>
public class Configuracao : EntidadeBase
{
    /// <summary>Valor do kWh em reais (usado para calcular custo de energia).</summary>
    public decimal KwhValor { get; private set; }

    /// <summary>Valor fixo mensal da agua cobrado de todos os inquilinos.</summary>
    public decimal ValorAgua { get; private set; }

    /// <summary>
    /// WalletId do Host (locador) na plataforma Asaas.
    /// Obtido ao registrar a subconta via <c>CriarSubcontaAsync</c>.
    /// Usado no split de pagamento PIX para direcionar o valor ao locador.
    /// </summary>
    public string? WalletIdAsaas { get; private set; }

    // ─── WhatsApp ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Número de WhatsApp do locador no formato internacional sem o '+' (ex: 5511999999999).
    /// Usado para gerar o link wa.me para contato com o inquilino.
    /// </summary>
    public string? NumeroWhatsappLocador { get; private set; }

    /// <summary>
    /// Template da mensagem padrão enviada via WhatsApp ao inquilino quando a fatura é enviada.
    /// Suporta os placeholders: {inquilino}, {mesReferencia}, {valorTotal}, {dataVencimento}, {codigoPix}.
    /// </summary>
    public string? MensagemPadraoWhatsapp { get; private set; }

    // ─── PIX Nativo (sem gateway) ──────────────────────────────────────────────

    /// <summary>Chave PIX do locador (CPF, CNPJ, e-mail, telefone ou chave aleatória).</summary>
    public string? ChavePix { get; private set; }

    /// <summary>Nome do recebedor conforme cadastro PIX (máx. 25 caracteres).</summary>
    public string? NomeRecebedorPix { get; private set; }

    /// <summary>Cidade do recebedor conforme cadastro PIX (máx. 15 caracteres).</summary>
    public string? CidadeRecebedorPix { get; private set; }

    protected Configuracao() { }

    public Configuracao(decimal kwhValor, decimal valorAgua)
    {
        ValidarValores(kwhValor, valorAgua);
        KwhValor = kwhValor;
        ValorAgua = valorAgua;
    }

    public void Atualizar(decimal kwhValor, decimal valorAgua)
    {
        ValidarValores(kwhValor, valorAgua);
        KwhValor = kwhValor;
        ValorAgua = valorAgua;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Armazena o WalletId retornado pelo Asaas apos o registro da subconta do host.
    /// </summary>
    public void AtualizarWalletIdAsaas(string walletId)
    {
        if (string.IsNullOrWhiteSpace(walletId))
            throw new ArgumentException("O WalletId do Asaas nao pode ser vazio.", nameof(walletId));
        WalletIdAsaas = walletId;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza as configurações de integração com WhatsApp.
    /// </summary>
    /// <param name="numero">Número no formato internacional sem '+' (ex: 5511999999999).</param>
    /// <param name="mensagemPadrao">Template da mensagem. Use {inquilino}, {mesReferencia}, {valorTotal}, {dataVencimento}, {codigoPix}.</param>
    public void AtualizarWhatsapp(string numero, string mensagemPadrao)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("O numero de WhatsApp nao pode ser vazio.", nameof(numero));
        if (string.IsNullOrWhiteSpace(mensagemPadrao))
            throw new ArgumentException("A mensagem padrao nao pode ser vazia.", nameof(mensagemPadrao));
        NumeroWhatsappLocador = numero.Trim();
        MensagemPadraoWhatsapp = mensagemPadrao.Trim();
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza as configurações de PIX nativo do locador.
    /// </summary>
    public void AtualizarPix(string chavePix, string nomeRecebedor, string cidadeRecebedor)
    {
        if (string.IsNullOrWhiteSpace(chavePix))
            throw new ArgumentException("A chave PIX nao pode ser vazia.", nameof(chavePix));
        if (string.IsNullOrWhiteSpace(nomeRecebedor))
            throw new ArgumentException("O nome do recebedor PIX nao pode ser vazio.", nameof(nomeRecebedor));
        if (string.IsNullOrWhiteSpace(cidadeRecebedor))
            throw new ArgumentException("A cidade do recebedor PIX nao pode ser vazia.", nameof(cidadeRecebedor));
        ChavePix = chavePix.Trim();
        NomeRecebedorPix = nomeRecebedor.Trim()[..Math.Min(nomeRecebedor.Trim().Length, 25)];
        CidadeRecebedorPix = cidadeRecebedor.Trim()[..Math.Min(cidadeRecebedor.Trim().Length, 15)];
        MarcarComoAtualizado();
    }

    private static void ValidarValores(decimal kwhValor, decimal valorAgua)
    {
        if (kwhValor < 0)
            throw new ArgumentException("O valor do kWh nao pode ser negativo.", nameof(kwhValor));
        if (valorAgua < 0)
            throw new ArgumentException("O valor da agua nao pode ser negativo.", nameof(valorAgua));
    }
}

