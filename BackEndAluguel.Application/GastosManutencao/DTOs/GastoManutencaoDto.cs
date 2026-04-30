namespace BackEndAluguel.Application.GastosManutencao.DTOs;

/// <summary>
/// DTO de leitura de um gasto de manutencao de apartamento.
/// </summary>
public record GastoManutencaoDto(
    /// <summary>Identificador unico do gasto.</summary>
    Guid Id,
    /// <summary>Identificador do apartamento ao qual o gasto pertence.</summary>
    Guid ApartamentoId,
    /// <summary>Numero do apartamento (preenchido quando disponivel).</summary>
    string? ApartamentoNumero,
    /// <summary>Descricao do servico ou material de manutencao.</summary>
    string Descricao,
    /// <summary>Valor total do gasto em reais.</summary>
    decimal Valor,
    /// <summary>Data em que o gasto ocorreu.</summary>
    DateOnly Data,
    /// <summary>Observacoes adicionais sobre o gasto.</summary>
    string? Observacao,
    /// <summary>Data de criacao do registro.</summary>
    DateTime CriadoEm
);

