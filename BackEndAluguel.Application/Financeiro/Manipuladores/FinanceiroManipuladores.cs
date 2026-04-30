using BackEndAluguel.Application.Financeiro.Consultas;
using BackEndAluguel.Application.Financeiro.DTOs;
using BackEndAluguel.Domain.Enumeradores;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Financeiro.Manipuladores;

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ObterBalancoMensalConsulta"/>.
/// Consolida receitas (faturas pagas) e gastos (manutencao) de um mes,
/// agrupando por apartamento para exibicao detalhada.
/// </summary>
public class ObterBalancoMensalManipulador : IRequestHandler<ObterBalancoMensalConsulta, BalancoMensalDto>
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IGastoManutencaoRepositorio _gastoRepositorio;

    /// <summary>Inicializa o manipulador com os repositorios de faturas e gastos.</summary>
    public ObterBalancoMensalManipulador(
        IFaturaRepositorio faturaRepositorio,
        IGastoManutencaoRepositorio gastoRepositorio)
    {
        _faturaRepositorio = faturaRepositorio;
        _gastoRepositorio = gastoRepositorio;
    }

    /// <summary>
    /// Processa a consulta de balanco mensal.
    /// Agrupa as faturas pagas e os gastos de manutencao por apartamento,
    /// calculando o balanco liquido de cada um e o total geral do mes.
    /// </summary>
    public async Task<BalancoMensalDto> Handle(ObterBalancoMensalConsulta request, CancellationToken cancellationToken)
    {
        // Formata o mes de referencia no padrao MM/AAAA para filtrar faturas
        var mesReferencia = $"{request.Mes:D2}/{request.Ano}";

        // Busca todas as faturas do mes com detalhes de inquilino e apartamento
        var todasFaturas = await _faturaRepositorio.ObterPorMesReferenciaComDetalhesAsync(mesReferencia, cancellationToken);

        // Filtra apenas as faturas pagas para compor as receitas
        var faturasPagas = todasFaturas.Where(f => f.Status == StatusFatura.Pago).ToList();

        // Busca todos os gastos de manutencao do mes com detalhes de apartamento
        var gastos = (await _gastoRepositorio.ObterPorMesAsync(request.Ano, request.Mes, cancellationToken)).ToList();

        // Agrupa as faturas pagas por apartamento (via Inquilino -> Apartamento)
        var faturasPorApartamento = faturasPagas
            .Where(f => f.Inquilino?.ApartamentoId != null)
            .GroupBy(f => f.Inquilino!.ApartamentoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Agrupa os gastos por apartamento
        var gastosPorApartamento = gastos
            .GroupBy(g => g.ApartamentoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Une os IDs de apartamento encontrados em faturas e gastos
        var todosApartamentosIds = faturasPorApartamento.Keys
            .Union(gastosPorApartamento.Keys)
            .Distinct()
            .ToList();

        // Monta o detalhe por apartamento
        var apartamentos = todosApartamentosIds.Select(aptId =>
        {
            var faturasDoApt = faturasPorApartamento.TryGetValue(aptId, out var fs) ? fs : new();
            var gastosDoApt = gastosPorApartamento.TryGetValue(aptId, out var gs) ? gs : new();

            var primeiraFatura = faturasDoApt.FirstOrDefault();
            var primeiroGasto = gastosDoApt.FirstOrDefault();

            // Recupera informacoes do apartamento a partir da primeira fatura ou gasto disponivel
            var numeroApt = primeiraFatura?.Inquilino?.Apartamento?.Numero
                         ?? primeiroGasto?.Apartamento?.Numero
                         ?? "Desconhecido";
            var blocoApt = primeiraFatura?.Inquilino?.Apartamento?.Bloco
                        ?? primeiroGasto?.Apartamento?.Bloco;

            var totalReceitas = faturasDoApt.Sum(f => f.CalcularValorTotal());
            var totalGastos = gastosDoApt.Sum(g => g.Valor);

            return new BalancoApartamentoDto(
                ApartamentoId: aptId,
                ApartamentoNumero: numeroApt,
                ApartamentoBloco: blocoApt,
                TotalReceitas: totalReceitas,
                TotalGastos: totalGastos,
                BalancoLiquido: totalReceitas - totalGastos,
                FaturasPagas: faturasDoApt.Select(f => new FaturaResumoDto(
                    f.Id,
                    f.MesReferencia,
                    f.CalcularValorTotal(),
                    f.DataPagamento,
                    f.Inquilino?.NomeCompleto ?? "Desconhecido"
                )),
                GastosManutencao: gastosDoApt.Select(g => new GastoResumoDto(
                    g.Id, g.Descricao, g.Valor, g.Data
                ))
            );
        }).OrderBy(a => a.ApartamentoNumero).ToList();

        var totalReceitasGeral = apartamentos.Sum(a => a.TotalReceitas);
        var totalGastosGeral = apartamentos.Sum(a => a.TotalGastos);

        return new BalancoMensalDto(
            MesReferencia: mesReferencia,
            Ano: request.Ano,
            Mes: request.Mes,
            TotalReceitas: totalReceitasGeral,
            TotalGastos: totalGastosGeral,
            BalancoLiquido: totalReceitasGeral - totalGastosGeral,
            Apartamentos: apartamentos
        );
    }
}

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ObterBalancoAnualConsulta"/>.
/// Retorna o resumo financeiro dos 12 meses do ano informado.
/// </summary>
public class ObterBalancoAnualManipulador : IRequestHandler<ObterBalancoAnualConsulta, BalancoAnualDto>
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IGastoManutencaoRepositorio _gastoRepositorio;

    /// <summary>Inicializa o manipulador com os repositorios necessarios.</summary>
    public ObterBalancoAnualManipulador(
        IFaturaRepositorio faturaRepositorio,
        IGastoManutencaoRepositorio gastoRepositorio)
    {
        _faturaRepositorio = faturaRepositorio;
        _gastoRepositorio = gastoRepositorio;
    }

    /// <summary>
    /// Processa a consulta de balanco anual.
    /// Para cada mes do ano, calcula o total de receitas (faturas pagas) e gastos.
    /// </summary>
    public async Task<BalancoAnualDto> Handle(ObterBalancoAnualConsulta request, CancellationToken cancellationToken)
    {
        // Busca todas as faturas pagas do ano com detalhes
        var faturasPagas = (await _faturaRepositorio.ObterPagasPorAnoAsync(request.Ano, cancellationToken)).ToList();

        // Busca todos os gastos do ano com detalhes
        var gastos = (await _gastoRepositorio.ObterPorAnoAsync(request.Ano, cancellationToken)).ToList();

        // Gera o resumo mensal para os 12 meses
        var meses = Enumerable.Range(1, 12).Select(mes =>
        {
            var mesRef = $"{mes:D2}/{request.Ano}";

            // Receitas: faturas pagas cujo MesReferencia corresponde ao mes/ano
            var receitasMes = faturasPagas
                .Where(f => f.MesReferencia == mesRef)
                .Sum(f => f.CalcularValorTotal());

            // Gastos: manutencoes cujo mes/ano da data de gasto corresponde ao mes
            var inicio = new DateOnly(request.Ano, mes, 1);
            var fim = inicio.AddMonths(1).AddDays(-1);
            var gastosMes = gastos
                .Where(g => g.Data >= inicio && g.Data <= fim)
                .Sum(g => g.Valor);

            return new BalancoMensalResumoDto(
                Mes: mes,
                MesReferencia: mesRef,
                TotalReceitas: receitasMes,
                TotalGastos: gastosMes,
                BalancoLiquido: receitasMes - gastosMes
            );
        }).ToList();

        var totalReceitas = meses.Sum(m => m.TotalReceitas);
        var totalGastos = meses.Sum(m => m.TotalGastos);

        return new BalancoAnualDto(
            Ano: request.Ano,
            TotalReceitas: totalReceitas,
            TotalGastos: totalGastos,
            BalancoLiquido: totalReceitas - totalGastos,
            Meses: meses
        );
    }
}

