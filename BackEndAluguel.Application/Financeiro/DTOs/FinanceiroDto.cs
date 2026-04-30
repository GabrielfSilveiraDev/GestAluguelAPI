namespace BackEndAluguel.Application.Financeiro.DTOs;

/// <summary>
/// DTO com o balanco financeiro detalhado de um apartamento em um mes especifico.
/// </summary>
public record BalancoApartamentoDto(
    /// <summary>Identificador do apartamento.</summary>
    Guid ApartamentoId,
    /// <summary>Numero do apartamento.</summary>
    string ApartamentoNumero,
    /// <summary>Bloco do apartamento (pode ser nulo).</summary>
    string? ApartamentoBloco,
    /// <summary>Total de receitas (faturas pagas) do apartamento no periodo.</summary>
    decimal TotalReceitas,
    /// <summary>Total de gastos de manutencao do apartamento no periodo.</summary>
    decimal TotalGastos,
    /// <summary>Balanco liquido: TotalReceitas - TotalGastos.</summary>
    decimal BalancoLiquido,
    /// <summary>Lista de faturas pagas no periodo.</summary>
    IEnumerable<FaturaResumoDto> FaturasPagas,
    /// <summary>Lista de gastos de manutencao no periodo.</summary>
    IEnumerable<GastoResumoDto> GastosManutencao
);

/// <summary>Resumo de uma fatura paga para o relatorio financeiro.</summary>
public record FaturaResumoDto(
    Guid FaturaId,
    string MesReferencia,
    decimal ValorTotal,
    DateOnly? DataPagamento,
    string NomeInquilino
);

/// <summary>Resumo de um gasto de manutencao para o relatorio financeiro.</summary>
public record GastoResumoDto(
    Guid GastoId,
    string Descricao,
    decimal Valor,
    DateOnly Data
);

/// <summary>
/// DTO com o balanco financeiro mensal consolidado de todos os apartamentos.
/// </summary>
public record BalancoMensalDto(
    /// <summary>Mes de referencia no formato "MM/AAAA".</summary>
    string MesReferencia,
    /// <summary>Ano do balanco.</summary>
    int Ano,
    /// <summary>Mes do balanco (1-12).</summary>
    int Mes,
    /// <summary>Total geral de receitas do mes (todas as faturas pagas).</summary>
    decimal TotalReceitas,
    /// <summary>Total geral de gastos de manutencao do mes.</summary>
    decimal TotalGastos,
    /// <summary>Balanco liquido total: TotalReceitas - TotalGastos.</summary>
    decimal BalancoLiquido,
    /// <summary>Detalhe por apartamento (preenchido quando expandido).</summary>
    IEnumerable<BalancoApartamentoDto> Apartamentos
);

/// <summary>
/// DTO com o resumo financeiro anual, por mes.
/// </summary>
public record BalancoAnualDto(
    /// <summary>Ano do balanco.</summary>
    int Ano,
    /// <summary>Total anual de receitas.</summary>
    decimal TotalReceitas,
    /// <summary>Total anual de gastos.</summary>
    decimal TotalGastos,
    /// <summary>Balanco liquido anual.</summary>
    decimal BalancoLiquido,
    /// <summary>Resumo mensal dos 12 meses.</summary>
    IEnumerable<BalancoMensalResumoDto> Meses
);

/// <summary>Resumo de um mes dentro do balanco anual (sem detalhes por apartamento).</summary>
public record BalancoMensalResumoDto(
    int Mes,
    string MesReferencia,
    decimal TotalReceitas,
    decimal TotalGastos,
    decimal BalancoLiquido
);

