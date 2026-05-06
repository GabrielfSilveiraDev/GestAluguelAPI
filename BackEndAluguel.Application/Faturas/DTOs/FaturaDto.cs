using BackEndAluguel.Domain.Enumeradores;

namespace BackEndAluguel.Application.Faturas.DTOs;

/// <summary>
/// Objeto de Transferência de Dados (DTO) para exposição das informações de uma fatura.
/// Utilizado como resultado de consultas (Queries) no padrão CQRS.
/// </summary>
public record FaturaDto(
    /// <summary>Identificador único da fatura.</summary>
    Guid Id,
    /// <summary>Mês de referência no formato "MM/AAAA".</summary>
    string MesReferencia,
    /// <summary>Valor do aluguel cobrado.</summary>
    decimal ValorAluguel,
    /// <summary>Valor da conta de água.</summary>
    decimal ValorAgua,
    /// <summary>Valor da conta de luz.</summary>
    decimal ValorLuz,
    /// <summary>Valor da garagem cobrado nesta fatura.</summary>
    decimal ValorGaragem,
    /// <summary>Valor total (aluguel + água + luz + garagem).</summary>
    decimal ValorTotal,
    /// <summary>Data limite para pagamento.</summary>
    DateOnly DataLimitePagamento,
    /// <summary>Data em que o pagamento foi realizado (nulo se não pago).</summary>
    DateOnly? DataPagamento,
    /// <summary>Código ou link do PIX gerado.</summary>
    string? CodigoPix,
    /// <summary>Status atual da fatura.</summary>
    StatusFatura Status,
    /// <summary>Descrição textual do status.</summary>
    string StatusDescricao,
    /// <summary>Identificador do inquilino responsável.</summary>
    Guid InquilinoId,
    /// <summary>Data de criação do registro.</summary>
    DateTime CriadoEm,
    /// <summary>Consumo de kWh do mês anterior.</summary>
    decimal? KwMesAnterior,
    /// <summary>Consumo de kWh atual.</summary>
    decimal? KwAtual,
    /// <summary>Total de kWh consumidos.</summary>
    decimal? KwConsumidos,
    /// <summary>Valor referente ao consumo de kWh.</summary>
    decimal? KwhValor,
    /// <summary>
    /// ID externo da cobrança gerada no gateway Asaas (ex: "pay_xxxx").
    /// Preenchido apos a chamada ao endpoint de geracao de PIX via Asaas.
    /// Nulo quando a cobranca ainda nao foi gerada.
    /// </summary>
    string? CobrancaAsaasId = null,
    /// <summary>Identificador do apartamento vinculado ao inquilino desta fatura.</summary>
    Guid? ApartamentoId = null,
    /// <summary>Número do apartamento (ex: "101").</summary>
    string? NumeroApartamento = null,
    /// <summary>Bloco do apartamento (ex: "A"). Nulo se não houver bloco.</summary>
    string? BlocoApartamento = null
);
