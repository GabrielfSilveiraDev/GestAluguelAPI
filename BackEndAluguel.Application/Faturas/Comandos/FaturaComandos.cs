using BackEndAluguel.Application.Faturas.DTOs;
using BackEndAluguel.Application.Pagamentos.DTOs;
using BackEndAluguel.Domain.Enumeradores;
using MediatR;

namespace BackEndAluguel.Application.Faturas.Comandos;

/// <summary>
/// Comando CQRS para geração de uma nova fatura mensal para um inquilino.
/// </summary>
/// <param name="InquilinoId">Identificador do inquilino.</param>
/// <param name="MesReferencia">Mês de referência no formato "MM/AAAA".</param>
/// <param name="ValorAluguel">Valor do aluguel a ser cobrado.</param>
/// <param name="DataLimitePagamento">Data limite para pagamento.</param>
/// <param name="KwAtual">Leitura atual em kWh (opcional).</param>
/// <param name="KwMesAnteriorManual">Leitura do mês anterior (override manual, opcional).</param>
/// <param name="ValorLuzManual">Valor da luz (override manual, opcional).</param>
/// <param name="ValorAguaManual">Valor da água (override manual, opcional).</param>
/// <param name="CodigoPix">Código ou link do PIX (opcional).</param>
/// <param name="ValorGaragem">Valor da garagem (override manual; se nulo, usa inquilino.Garagem).</param>
public record CriarFaturaComando(
    Guid InquilinoId,
    string MesReferencia,
    decimal ValorAluguel,
    DateOnly DataLimitePagamento,
    decimal? KwAtual = null,
    decimal? KwMesAnteriorManual = null,   // override da busca automatica (para primeira fatura)
    decimal? ValorLuzManual = null,         // override do calculo automatico
    decimal? ValorAguaManual = null,        // override do valor da config
    string? CodigoPix = null,
    decimal? ValorGaragem = null            // se nulo, usa inquilino.Garagem automaticamente
) : IRequest<FaturaDto>;

/// <summary>
/// Comando CQRS para registrar o pagamento de uma fatura existente.
/// </summary>
/// <param name="Id">Identificador único da fatura.</param>
/// <param name="DataPagamento">Data em que o pagamento foi realizado.</param>
public record RegistrarPagamentoComando(
    Guid Id,
    DateOnly DataPagamento
) : IRequest<FaturaDto>;

/// <summary>
/// Comando CQRS para atualizar os valores de água e luz de uma fatura ainda não paga.
/// </summary>
/// <param name="Id">Identificador único da fatura.</param>
/// <param name="ValorAgua">Novo valor da conta de água.</param>
/// <param name="ValorLuz">Novo valor da conta de luz.</param>
public record AtualizarValoresConsumoComando(
    Guid Id,
    decimal ValorAgua,
    decimal ValorLuz
) : IRequest<FaturaDto>;

/// <summary>
/// Comando CQRS para atualizar a leitura de kWh e recalcular o valor da luz.
/// </summary>
/// <param name="Id">Identificador único da fatura.</param>
/// <param name="KwAtual">Nova leitura em kWh.</param>
/// <param name="KwhValorOverride">Valor em R$ para o kWh (opcional).</param>
public record AtualizarLeituraKwComando(
    Guid Id,
    decimal KwAtual,
    decimal? KwhValorOverride = null
) : IRequest<FaturaDto>;

/// <summary>
/// Comando CQRS para atualizar o código PIX de uma fatura.
/// </summary>
/// <param name="Id">Identificador único da fatura.</param>
/// <param name="CodigoPix">Novo código ou link do PIX.</param>
public record AtualizarCodigoPixComando(
    Guid Id,
    string CodigoPix
) : IRequest<FaturaDto>;

/// <summary>
/// Comando CQRS para processar automaticamente faturas vencidas,
/// alterando o status para Atrasado quando a data limite for ultrapassada.
/// Retorna a quantidade de faturas marcadas como atrasadas.
/// </summary>
public record ProcessarFaturasVencidasComando() : IRequest<int>;

/// <summary>
/// Comando CQRS para gerar uma cobrança PIX via gateway Asaas para uma fatura existente.
/// O manipulador busca o WalletId do host na Configuracao global, chama a API do Asaas,
/// salva o CobrancaAsaasId e o CodigoPix (copia-e-cola) na fatura e retorna os dados de pagamento.
/// </summary>
/// <param name="FaturaId">Identificador unico da fatura a ser cobrada.</param>
/// <param name="PercentualSplit">
/// Percentual a ser direcionado ao WalletId do host (0-100).
/// Se nulo, o valor total e direcionado.
/// </param>
public record GerarCobrancaPixComando(
    Guid FaturaId,
    decimal? PercentualSplit = null
) : IRequest<CobrancaPixResultadoDto>;
