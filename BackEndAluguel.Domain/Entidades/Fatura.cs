using BackEndAluguel.Domain.Enumeradores;

namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Entidade que representa uma fatura mensal de aluguel gerada para um inquilino.
/// Agrupa todos os valores cobrados em um determinado mês de referência,
/// bem como o controle de pagamento e as informações do PIX gerado.
///
/// Relacionamentos:
/// - N Faturas para 1 Inquilino (chave estrangeira InquilinoId).
///   Cada fatura pertence exclusivamente a um inquilino.
/// </summary>
public class Fatura : EntidadeBase
{
    /// <summary>
    /// Mês de referência da fatura no formato "MM/AAAA" (ex: "05/2025").
    /// Utilizado para identificar qual competência a fatura está cobrindo.
    /// </summary>
    public string MesReferencia { get; private set; } = string.Empty;

    /// <summary>
    /// Valor do aluguel mensal cobrado nesta fatura (em reais).
    /// Pode diferir do valor contratual em caso de reajuste ou negociação.
    /// </summary>
    public decimal ValorAluguel { get; private set; }

    /// <summary>
    /// Valor da conta de água referente ao mês de competência (em reais).
    /// </summary>
    public decimal ValorAgua { get; private set; }

    /// <summary>
    /// Valor da conta de luz (energia elétrica) referente ao mês de competência (em reais).
    /// </summary>
    public decimal ValorLuz { get; private set; }

    /// <summary>Leitura do kWh no mes anterior (inicio do periodo).</summary>
    public decimal? KwMesAnterior { get; private set; }

    /// <summary>Leitura do kWh no mes atual (fim do periodo).</summary>
    public decimal? KwAtual { get; private set; }

    /// <summary>Valor do kWh (tarifa) vigente no momento da geracao da fatura.</summary>
    public decimal? KwhValor { get; private set; }

    /// <summary>Consumo do periodo em kWh. Calculado: KwAtual - KwMesAnterior.</summary>
    public decimal? KwConsumidos =>
        KwAtual.HasValue && KwMesAnterior.HasValue
            ? KwAtual.Value - KwMesAnterior.Value
            : null;

    /// <summary>
    /// Data limite para que o pagamento seja realizado sem incidência de multa ou juros.
    /// </summary>
    public DateOnly DataLimitePagamento { get; private set; }

    /// <summary>
    /// Data em que o pagamento foi efetivamente realizado.
    /// Nulo quando a fatura ainda não foi paga.
    /// </summary>
    public DateOnly? DataPagamento { get; private set; }

    /// <summary>
    /// Código ou link do PIX gerado para facilitar o pagamento desta fatura.
    /// Pode conter a chave PIX copia-e-cola ou URL de pagamento.
    /// </summary>
    public string? CodigoPix { get; private set; }

    /// <summary>
    /// Identificador externo da cobrança no gateway de pagamento Asaas.
    /// Utilizado para correlacionar o webhook de confirmação de pagamento com a fatura.
    /// </summary>
    public string? CobrancaAsaasId { get; private set; }

    /// <summary>
    /// Status atual da fatura: Pendente, Atrasado ou Pago.
    /// Controla o ciclo de vida do pagamento mensal.
    /// </summary>
    public StatusFatura Status { get; private set; }

    // =============================================================
    // RELACIONAMENTO: N Faturas -> 1 Inquilino
    // Uma fatura sempre pertence a um único inquilino.
    // A chave estrangeira InquilinoId referencia a tabela Inquilinos.
    // =============================================================

    /// <summary>
    /// Identificador do inquilino a quem esta fatura pertence.
    /// Chave estrangeira para a entidade Inquilino.
    /// </summary>
    public Guid InquilinoId { get; private set; }

    /// <summary>
    /// Propriedade de navegação para o inquilino vinculado.
    /// Permite acessar os dados do inquilino diretamente através da fatura.
    /// </summary>
    public Inquilino? Inquilino { get; private set; }

    /// <summary>
    /// Construtor protegido requerido pelo Entity Framework Core.
    /// </summary>
    protected Fatura() { }

    /// <summary>
    /// Cria uma nova fatura mensal para um inquilino.
    /// O status inicial é sempre <see cref="StatusFatura.Pendente"/>.
    /// </summary>
    /// <param name="mesReferencia">Mês de referência no formato "MM/AAAA".</param>
    /// <param name="valorAluguel">Valor do aluguel a ser cobrado.</param>
    /// <param name="valorAgua">Valor da conta de água.</param>
    /// <param name="valorLuz">Valor da conta de luz.</param>
    /// <param name="dataLimitePagamento">Data limite para pagamento sem multa.</param>
    /// <param name="inquilinoId">Identificador do inquilino responsável pelo pagamento.</param>
    /// <param name="kwMesAnterior">Leitura do kWh no mês anterior (opcional).</param>
    /// <param name="kwAtual">Leitura do kWh no mês atual (opcional).</param>
    /// <param name="kwhValor">Valor do kWh vigente no momento da geração da fatura (opcional).</param>
    /// <param name="codigoPix">Código ou link PIX gerado para pagamento (opcional).</param>
    public Fatura(
        string mesReferencia,
        decimal valorAluguel,
        decimal valorAgua,
        decimal valorLuz,
        DateOnly dataLimitePagamento,
        Guid inquilinoId,
        decimal? kwMesAnterior = null,
        decimal? kwAtual = null,
        decimal? kwhValor = null,
        string? codigoPix = null)
    {
        ValidarMesReferencia(mesReferencia);
        if (valorAluguel <= 0)
            throw new ArgumentException("O valor do aluguel deve ser maior que zero.", nameof(valorAluguel));
        if (valorAgua < 0)
            throw new ArgumentException("O valor da agua nao pode ser negativo.", nameof(valorAgua));
        if (valorLuz < 0)
            throw new ArgumentException("O valor da luz nao pode ser negativo.", nameof(valorLuz));

        MesReferencia = mesReferencia.Trim();
        ValorAluguel = valorAluguel;
        ValorAgua = valorAgua;
        ValorLuz = valorLuz;
        DataLimitePagamento = dataLimitePagamento;
        InquilinoId = inquilinoId;
        KwMesAnterior = kwMesAnterior;
        KwAtual = kwAtual;
        KwhValor = kwhValor;
        CodigoPix = codigoPix;
        Status = StatusFatura.Pendente;
    }

    /// <summary>
    /// Calcula o valor total da fatura somando aluguel, água e luz.
    /// </summary>
    /// <returns>Soma de todos os valores cobrados na fatura.</returns>
    public decimal CalcularValorTotal() => ValorAluguel + ValorAgua + ValorLuz;

    /// <summary>
    /// Registra o pagamento da fatura, atualizando a data de pagamento e o status para Pago.
    /// </summary>
    /// <param name="dataPagamento">Data em que o pagamento foi realizado.</param>
    /// <exception cref="InvalidOperationException">Lançado se a fatura já estiver paga.</exception>
    public void RegistrarPagamento(DateOnly dataPagamento)
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Esta fatura ja foi paga.");
        DataPagamento = dataPagamento;
        Status = StatusFatura.Pago;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza o status da fatura para Atrasado.
    /// Deve ser chamado quando a data limite de pagamento for ultrapassada sem pagamento.
    /// </summary>
    /// <exception cref="InvalidOperationException">Lançado se a fatura já estiver paga.</exception>
    public void MarcarComoAtrasado()
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Nao e possivel marcar como atrasada uma fatura ja paga.");
        Status = StatusFatura.Atrasado;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza o código/link PIX associado a esta fatura.
    /// </summary>
    /// <param name="codigoPix">Novo código ou link do PIX.</param>
    public void AtualizarCodigoPix(string codigoPix)
    {
        CodigoPix = codigoPix;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Registra o identificador de cobrança retornado pelo gateway Asaas.
    /// Permite rastrear a fatura via webhook quando o pagamento for confirmado.
    /// </summary>
    /// <param name="cobrancaAsaasId">ID da cobrança no Asaas (ex: "pay_xxxxxxxxxxxx").</param>
    public void RegistrarCobrancaAsaas(string cobrancaAsaasId)
    {
        CobrancaAsaasId = cobrancaAsaasId;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza os valores de água e luz da fatura antes do pagamento.
    /// </summary>
    /// <param name="valorAgua">Novo valor da água.</param>
    /// <param name="valorLuz">Novo valor da luz.</param>
    /// <exception cref="InvalidOperationException">Lançado se a fatura já estiver paga.</exception>
    public void AtualizarValoresConsumo(decimal valorAgua, decimal valorLuz)
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Nao e possivel alterar valores de uma fatura ja paga.");
        if (valorAgua < 0) throw new ArgumentException("O valor da agua nao pode ser negativo.", nameof(valorAgua));
        if (valorLuz < 0) throw new ArgumentException("O valor da luz nao pode ser negativo.", nameof(valorLuz));
        ValorAgua = valorAgua;
        ValorLuz = valorLuz;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza a leitura de kWh e recalcula o ValorLuz automaticamente se possivel.
    /// </summary>
    public void AtualizarLeituraKw(decimal kwAtual, decimal? kwhValorOverride = null)
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Nao e possivel alterar valores de uma fatura ja paga.");

        KwAtual = kwAtual;
        if (kwhValorOverride.HasValue) KwhValor = kwhValorOverride.Value;

        // Auto-recalcular ValorLuz se todos os dados estao disponiveis
        if (KwAtual.HasValue && KwMesAnterior.HasValue && KwhValor.HasValue)
        {
            var consumido = KwAtual.Value - KwMesAnterior.Value;
            if (consumido >= 0)
                ValorLuz = consumido * KwhValor.Value;
        }

        MarcarComoAtualizado();
    }

    /// <summary>
    /// Verifica se a fatura está vencida (data limite ultrapassada e não paga).
    /// </summary>
    /// <returns>Verdadeiro se a fatura estiver vencida e não paga.</returns>
    public bool EstaVencida()
        => Status != StatusFatura.Pago && DataLimitePagamento < DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Valida o formato do mês de referência (MM/AAAA).
    /// </summary>
    private static void ValidarMesReferencia(string mesReferencia)
    {
        if (string.IsNullOrWhiteSpace(mesReferencia))
            throw new ArgumentException("O mes de referencia nao pode ser vazio.", nameof(mesReferencia));
        if (!System.Text.RegularExpressions.Regex.IsMatch(mesReferencia.Trim(), @"^(0[1-9]|1[0-2])/\d{4}$"))
            throw new ArgumentException("O mes de referencia deve estar no formato MM/AAAA.", nameof(mesReferencia));
    }
}
