using BackEndAluguel.Application.Contratos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Contratos.Consultas;

/// <summary>Consulta CQRS para obter os metadados de um contrato pelo ID.</summary>
public record ObterContratoPorIdConsulta(Guid Id) : IRequest<ContratoInquilinoDto?>;

/// <summary>Consulta CQRS para listar todos os contratos de um inquilino.</summary>
public record ListarContratosPorInquilinoConsulta(Guid InquilinoId) : IRequest<IEnumerable<ContratoInquilinoDto>>;

