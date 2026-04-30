using BackEndAluguel.Application.Financeiro.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Financeiro.Consultas;

/// <summary>
/// Consulta CQRS para obter o balanco financeiro mensal detalhado por apartamento.
/// </summary>
/// <param name="Ano">Ano de referencia.</param>
/// <param name="Mes">Mes de referencia (1-12).</param>
public record ObterBalancoMensalConsulta(int Ano, int Mes) : IRequest<BalancoMensalDto>;

/// <summary>
/// Consulta CQRS para obter o balanco financeiro anual com resumo mensal.
/// </summary>
/// <param name="Ano">Ano de referencia.</param>
public record ObterBalancoAnualConsulta(int Ano) : IRequest<BalancoAnualDto>;

