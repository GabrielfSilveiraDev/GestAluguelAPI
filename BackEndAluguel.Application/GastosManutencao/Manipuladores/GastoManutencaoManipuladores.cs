using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Application.GastosManutencao.Comandos;
using BackEndAluguel.Application.GastosManutencao.Consultas;
using BackEndAluguel.Application.GastosManutencao.DTOs;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.GastosManutencao.Manipuladores;

/// <summary>Manipulador CQRS para criacao de um novo gasto de manutencao.</summary>
public class CriarGastoManutencaoManipulador : IRequestHandler<CriarGastoManutencaoComando, GastoManutencaoDto>
{
    private readonly IGastoManutencaoRepositorio _gastoRepositorio;
    private readonly IApartamentoRepositorio _apartamentoRepositorio;

    /// <summary>Inicializa o manipulador com os repositorios necessarios.</summary>
    public CriarGastoManutencaoManipulador(IGastoManutencaoRepositorio gastoRepositorio, IApartamentoRepositorio apartamentoRepositorio)
    {
        _gastoRepositorio = gastoRepositorio;
        _apartamentoRepositorio = apartamentoRepositorio;
    }

    /// <summary>Processa a criacao do gasto, validando a existencia do apartamento.</summary>
    public async Task<GastoManutencaoDto> Handle(CriarGastoManutencaoComando request, CancellationToken cancellationToken)
    {
        var apartamento = await _apartamentoRepositorio.ObterPorIdAsync(request.ApartamentoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Apartamento), request.ApartamentoId);

        var gasto = new Domain.Entidades.GastoManutencao(
            request.ApartamentoId, request.Descricao, request.Valor, request.Data, request.Observacao);

        await _gastoRepositorio.AdicionarAsync(gasto, cancellationToken);
        await _gastoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return ConverterParaDto(gasto, apartamento.Numero);
    }

    /// <summary>Converte a entidade para DTO de resposta.</summary>
    internal static GastoManutencaoDto ConverterParaDto(Domain.Entidades.GastoManutencao g, string? numeroApartamento = null)
        => new(g.Id, g.ApartamentoId, numeroApartamento ?? g.Apartamento?.Numero,
               g.Descricao, g.Valor, g.Data, g.Observacao, g.CriadoEm);
}

/// <summary>Manipulador CQRS para atualizacao de um gasto de manutencao existente.</summary>
public class AtualizarGastoManutencaoManipulador : IRequestHandler<AtualizarGastoManutencaoComando, GastoManutencaoDto>
{
    private readonly IGastoManutencaoRepositorio _repositorio;
    public AtualizarGastoManutencaoManipulador(IGastoManutencaoRepositorio repositorio) { _repositorio = repositorio; }

    /// <summary>Processa a atualizacao do gasto.</summary>
    public async Task<GastoManutencaoDto> Handle(AtualizarGastoManutencaoComando request, CancellationToken cancellationToken)
    {
        var gasto = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Domain.Entidades.GastoManutencao), request.Id);

        gasto.Atualizar(request.Descricao, request.Valor, request.Data, request.Observacao);
        _repositorio.Atualizar(gasto);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return CriarGastoManutencaoManipulador.ConverterParaDto(gasto);
    }
}

/// <summary>Manipulador CQRS para remocao de um gasto de manutencao.</summary>
public class RemoverGastoManutencaoManipulador : IRequestHandler<RemoverGastoManutencaoComando, bool>
{
    private readonly IGastoManutencaoRepositorio _repositorio;
    public RemoverGastoManutencaoManipulador(IGastoManutencaoRepositorio repositorio) { _repositorio = repositorio; }

    /// <summary>Processa a remocao do gasto.</summary>
    public async Task<bool> Handle(RemoverGastoManutencaoComando request, CancellationToken cancellationToken)
    {
        var gasto = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Domain.Entidades.GastoManutencao), request.Id);

        _repositorio.Remover(gasto);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

/// <summary>Manipulador CQRS para obter gasto por ID.</summary>
public class ObterGastoManutencaoPorIdManipulador : IRequestHandler<ObterGastoManutencaoPorIdConsulta, GastoManutencaoDto?>
{
    private readonly IGastoManutencaoRepositorio _repositorio;
    public ObterGastoManutencaoPorIdManipulador(IGastoManutencaoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<GastoManutencaoDto?> Handle(ObterGastoManutencaoPorIdConsulta request, CancellationToken cancellationToken)
    {
        var gasto = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return gasto is null ? null : CriarGastoManutencaoManipulador.ConverterParaDto(gasto);
    }
}

/// <summary>Manipulador CQRS para listar gastos por apartamento.</summary>
public class ListarGastosPorApartamentoManipulador : IRequestHandler<ListarGastosPorApartamentoConsulta, IEnumerable<GastoManutencaoDto>>
{
    private readonly IGastoManutencaoRepositorio _repositorio;
    public ListarGastosPorApartamentoManipulador(IGastoManutencaoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<GastoManutencaoDto>> Handle(ListarGastosPorApartamentoConsulta request, CancellationToken cancellationToken)
    {
        var gastos = await _repositorio.ObterPorApartamentoAsync(request.ApartamentoId, cancellationToken);
        return gastos.Select(g => CriarGastoManutencaoManipulador.ConverterParaDto(g));
    }
}

/// <summary>Manipulador CQRS para listar gastos por mes.</summary>
public class ListarGastosPorMesManipulador : IRequestHandler<ListarGastosPorMesConsulta, IEnumerable<GastoManutencaoDto>>
{
    private readonly IGastoManutencaoRepositorio _repositorio;
    public ListarGastosPorMesManipulador(IGastoManutencaoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<GastoManutencaoDto>> Handle(ListarGastosPorMesConsulta request, CancellationToken cancellationToken)
    {
        var gastos = await _repositorio.ObterPorMesAsync(request.Ano, request.Mes, cancellationToken);
        return gastos.Select(g => CriarGastoManutencaoManipulador.ConverterParaDto(g, g.Apartamento?.Numero));
    }
}

/// <summary>Manipulador CQRS para listar gastos por ano.</summary>
public class ListarGastosPorAnoManipulador : IRequestHandler<ListarGastosPorAnoConsulta, IEnumerable<GastoManutencaoDto>>
{
    private readonly IGastoManutencaoRepositorio _repositorio;
    public ListarGastosPorAnoManipulador(IGastoManutencaoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<GastoManutencaoDto>> Handle(ListarGastosPorAnoConsulta request, CancellationToken cancellationToken)
    {
        var gastos = await _repositorio.ObterPorAnoAsync(request.Ano, cancellationToken);
        return gastos.Select(g => CriarGastoManutencaoManipulador.ConverterParaDto(g, g.Apartamento?.Numero));
    }
}

