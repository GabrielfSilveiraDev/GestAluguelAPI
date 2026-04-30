using BackEndAluguel.Application.Apartamentos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Apartamentos.Consultas;

/// <summary>
/// Consulta CQRS para obter os dados de um apartamento pelo seu identificador único.
/// Segue o padrão Query do CQRS — representa uma intenção de leitura sem alterar o estado.
/// </summary>
/// <param name="Id">Identificador único do apartamento.</param>
public record ObterApartamentoPorIdConsulta(Guid Id) : IRequest<ApartamentoDto?>;

/// <summary>
/// Consulta CQRS para listar todos os apartamentos cadastrados.
/// </summary>
public record ListarApartamentosConsulta() : IRequest<IEnumerable<ApartamentoDto>>;

/// <summary>
/// Consulta CQRS para listar todos os apartamentos que estão desocupados (disponíveis para locação).
/// </summary>
public record ListarApartamentosDesocupadosConsulta() : IRequest<IEnumerable<ApartamentoDto>>;

