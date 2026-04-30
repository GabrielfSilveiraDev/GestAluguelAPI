using BackEndAluguel.Application.Configuracoes.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Configuracoes.Consultas;

/// <summary>Consulta CQRS para obter a configuracao global do sistema.</summary>
public record ObterConfiguracaoConsulta() : IRequest<ConfiguracaoDto?>;

