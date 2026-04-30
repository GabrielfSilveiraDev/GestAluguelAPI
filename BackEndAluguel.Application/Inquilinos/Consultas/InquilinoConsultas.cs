using BackEndAluguel.Application.Inquilinos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Inquilinos.Consultas;

public record ObterInquilinoPorIdConsulta(Guid Id) : IRequest<InquilinoDto?>;
public record ListarInquilinosConsulta() : IRequest<IEnumerable<InquilinoDto>>;
public record ListarInquilinosPorApartamentoConsulta(Guid ApartamentoId) : IRequest<IEnumerable<InquilinoDto>>;
public record ListarInquilinosComContratoProximoConsulta(int DiasAntecedencia) : IRequest<IEnumerable<InquilinoDto>>;

// Dependentes
public record ListarDependentesPorInquilinoConsulta(Guid InquilinoId) : IRequest<IEnumerable<DependenteDto>>;
public record ObterDependentePorIdConsulta(Guid Id) : IRequest<DependenteDto?>;


