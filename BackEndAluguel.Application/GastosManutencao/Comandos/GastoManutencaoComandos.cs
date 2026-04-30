using BackEndAluguel.Application.GastosManutencao.DTOs;
using MediatR;

namespace BackEndAluguel.Application.GastosManutencao.Comandos;

/// <summary>Comando CQRS para registrar um novo gasto de manutencao em um apartamento.</summary>
public record CriarGastoManutencaoComando(
    Guid ApartamentoId,
    string Descricao,
    decimal Valor,
    DateOnly Data,
    string? Observacao = null
) : IRequest<GastoManutencaoDto>;

/// <summary>Comando CQRS para atualizar um gasto de manutencao existente.</summary>
public record AtualizarGastoManutencaoComando(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly Data,
    string? Observacao = null
) : IRequest<GastoManutencaoDto>;

/// <summary>Comando CQRS para remover um gasto de manutencao. Retorna verdadeiro se bem-sucedido.</summary>
public record RemoverGastoManutencaoComando(Guid Id) : IRequest<bool>;

