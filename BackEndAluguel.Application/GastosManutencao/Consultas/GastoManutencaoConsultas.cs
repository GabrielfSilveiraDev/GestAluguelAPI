using BackEndAluguel.Application.GastosManutencao.DTOs;
using MediatR;

namespace BackEndAluguel.Application.GastosManutencao.Consultas;

/// <summary>Consulta CQRS para obter um gasto pelo ID.</summary>
public record ObterGastoManutencaoPorIdConsulta(Guid Id) : IRequest<GastoManutencaoDto?>;

/// <summary>Consulta CQRS para listar todos os gastos de um apartamento especifico.</summary>
public record ListarGastosPorApartamentoConsulta(Guid ApartamentoId) : IRequest<IEnumerable<GastoManutencaoDto>>;

/// <summary>Consulta CQRS para listar gastos de manutencao de um mes/ano especifico.</summary>
public record ListarGastosPorMesConsulta(int Ano, int Mes) : IRequest<IEnumerable<GastoManutencaoDto>>;

/// <summary>Consulta CQRS para listar todos os gastos de um ano.</summary>
public record ListarGastosPorAnoConsulta(int Ano) : IRequest<IEnumerable<GastoManutencaoDto>>;

