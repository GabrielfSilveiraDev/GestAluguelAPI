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
    /// <param name="walletId">WalletId fornecido pela API do Asaas.</param>
    public void AtualizarWalletIdAsaas(string walletId)
    {
        if (string.IsNullOrWhiteSpace(walletId))
            throw new ArgumentException("O WalletId do Asaas nao pode ser vazio.", nameof(walletId));
        WalletIdAsaas = walletId;
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

