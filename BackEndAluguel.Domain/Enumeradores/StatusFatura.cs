namespace BackEndAluguel.Domain.Enumeradores;

/// <summary>
/// Enumeração que representa os possíveis estados de uma fatura de aluguel.
/// Utilizada para controlar o ciclo de vida do pagamento mensal.
/// </summary>
public enum StatusFatura
{
    /// <summary>
    /// Fatura gerada, aguardando pagamento dentro do prazo.
    /// </summary>
    Pendente = 1,

    /// <summary>
    /// Fatura não foi paga após a data limite de pagamento.
    /// </summary>
    Atrasado = 2,

    /// <summary>
    /// Fatura quitada com sucesso.
    /// </summary>
    Pago = 3
}

