using BackEndAluguel.Application.Faturas.DTOs;
using BackEndAluguel.Domain.Enumeradores;
using MediatR;

namespace BackEndAluguel.Application.Faturas.Consultas;

/// <summary>
/// Consulta CQRS para obter os dados de uma fatura pelo seu identificador único.
/// </summary>
/// <param name="Id">Identificador único da fatura.</param>
public record ObterFaturaPorIdConsulta(Guid Id) : IRequest<FaturaDto?>;

/// <summary>
/// Consulta CQRS para listar todas as faturas de um inquilino específico.
/// </summary>
/// <param name="InquilinoId">Identificador do inquilino.</param>
public record ListarFaturasPorInquilinoConsulta(Guid InquilinoId) : IRequest<IEnumerable<FaturaDto>>;

/// <summary>
/// Consulta CQRS para listar faturas filtradas por status (Pendente, Atrasado ou Pago).
/// </summary>
/// <param name="Status">Status a ser filtrado.</param>
public record ListarFaturasPorStatusConsulta(StatusFatura Status) : IRequest<IEnumerable<FaturaDto>>;

/// <summary>
/// Consulta CQRS para listar todas as faturas vencidas e não pagas do sistema.
/// </summary>
public record ListarFaturasVencidasConsulta() : IRequest<IEnumerable<FaturaDto>>;

