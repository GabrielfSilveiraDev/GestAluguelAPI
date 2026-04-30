using BackEndAluguel.Application.Apartamentos.Comandos;
using BackEndAluguel.Application.Apartamentos.Consultas;
using BackEndAluguel.Application.Apartamentos.DTOs;
using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Apartamentos.Manipuladores;

/// <summary>
/// Manipulador CQRS responsável por processar o comando <see cref="CriarApartamentoComando"/>.
/// Implementa a lógica de criação de um novo apartamento, validando duplicidades.
/// </summary>
public class CriarApartamentoManipulador : IRequestHandler<CriarApartamentoComando, ApartamentoDto>
{
    private readonly IApartamentoRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de apartamentos via injeção de dependência.
    /// </summary>
    /// <param name="repositorio">Repositório de apartamentos.</param>
    public CriarApartamentoManipulador(IApartamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa o comando de criação, valida existência prévia e persiste o novo apartamento.
    /// </summary>
    /// <param name="request">Dados do apartamento a ser criado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>DTO com os dados do apartamento criado.</returns>
    public async Task<ApartamentoDto> Handle(CriarApartamentoComando request, CancellationToken cancellationToken)
    {
        var blocoNormalizado = string.IsNullOrWhiteSpace(request.Bloco) ? string.Empty : request.Bloco.Trim().ToUpper();
        var jaExiste = await _repositorio.ExisteAsync(request.Numero, blocoNormalizado, cancellationToken);
        if (jaExiste)
            throw new RegraDeNegocioExcecao($"Ja existe um apartamento com o numero '{request.Numero}'" +
                (string.IsNullOrEmpty(blocoNormalizado) ? "." : $" no bloco '{blocoNormalizado}'."));

        var apartamento = new Apartamento(request.Numero, request.Bloco);
        await _repositorio.AdicionarAsync(apartamento, cancellationToken);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return ConverterParaDto(apartamento);
    }

    /// <summary>
    /// Converte uma entidade Apartamento para o DTO de resposta.
    /// </summary>
    internal static ApartamentoDto ConverterParaDto(Apartamento ap)
        => new(ap.Id, ap.Numero, ap.Bloco, ap.Ocupado, ap.CriadoEm);
}

/// <summary>
/// Manipulador CQRS responsável por processar o comando <see cref="AtualizarApartamentoComando"/>.
/// </summary>
public class AtualizarApartamentoManipulador : IRequestHandler<AtualizarApartamentoComando, ApartamentoDto>
{
    private readonly IApartamentoRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de apartamentos.
    /// </summary>
    public AtualizarApartamentoManipulador(IApartamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa o comando de atualização, buscando o apartamento e aplicando as mudanças.
    /// </summary>
    public async Task<ApartamentoDto> Handle(AtualizarApartamentoComando request, CancellationToken cancellationToken)
    {
        var apartamento = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Apartamento), request.Id);

        apartamento.Atualizar(request.Numero, request.Bloco);
        _repositorio.Atualizar(apartamento);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return CriarApartamentoManipulador.ConverterParaDto(apartamento);
    }
}

/// <summary>
/// Manipulador CQRS responsável por processar o comando <see cref="RemoverApartamentoComando"/>.
/// </summary>
public class RemoverApartamentoManipulador : IRequestHandler<RemoverApartamentoComando, bool>
{
    private readonly IApartamentoRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de apartamentos.
    /// </summary>
    public RemoverApartamentoManipulador(IApartamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa o comando de remoção. Não permite remover apartamentos ocupados.
    /// </summary>
    public async Task<bool> Handle(RemoverApartamentoComando request, CancellationToken cancellationToken)
    {
        var apartamento = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Apartamento), request.Id);

        if (apartamento.Ocupado)
            throw new RegraDeNegocioExcecao("Não é possível remover um apartamento que está ocupado.");

        _repositorio.Remover(apartamento);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Manipulador CQRS responsável por processar a consulta <see cref="ObterApartamentoPorIdConsulta"/>.
/// </summary>
public class ObterApartamentoPorIdManipulador : IRequestHandler<ObterApartamentoPorIdConsulta, ApartamentoDto?>
{
    private readonly IApartamentoRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de apartamentos.
    /// </summary>
    public ObterApartamentoPorIdManipulador(IApartamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a consulta de busca por ID, retornando o DTO ou nulo se não encontrado.
    /// </summary>
    public async Task<ApartamentoDto?> Handle(ObterApartamentoPorIdConsulta request, CancellationToken cancellationToken)
    {
        var apartamento = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return apartamento is null ? null : CriarApartamentoManipulador.ConverterParaDto(apartamento);
    }
}

/// <summary>
/// Manipulador CQRS responsável por processar a consulta <see cref="ListarApartamentosConsulta"/>.
/// </summary>
public class ListarApartamentosManipulador : IRequestHandler<ListarApartamentosConsulta, IEnumerable<ApartamentoDto>>
{
    private readonly IApartamentoRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de apartamentos.
    /// </summary>
    public ListarApartamentosManipulador(IApartamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a consulta de listagem, retornando todos os apartamentos como DTOs.
    /// </summary>
    public async Task<IEnumerable<ApartamentoDto>> Handle(ListarApartamentosConsulta request, CancellationToken cancellationToken)
    {
        var apartamentos = await _repositorio.ObterTodosAsync(cancellationToken);
        return apartamentos.Select(CriarApartamentoManipulador.ConverterParaDto);
    }
}

/// <summary>
/// Manipulador CQRS responsável por processar a consulta <see cref="ListarApartamentosDesocupadosConsulta"/>.
/// </summary>
public class ListarApartamentosDesocupadosManipulador : IRequestHandler<ListarApartamentosDesocupadosConsulta, IEnumerable<ApartamentoDto>>
{
    private readonly IApartamentoRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de apartamentos.
    /// </summary>
    public ListarApartamentosDesocupadosManipulador(IApartamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a consulta de apartamentos desocupados.
    /// </summary>
    public async Task<IEnumerable<ApartamentoDto>> Handle(ListarApartamentosDesocupadosConsulta request, CancellationToken cancellationToken)
    {
        var apartamentos = await _repositorio.ObterDesocupadosAsync(cancellationToken);
        return apartamentos.Select(CriarApartamentoManipulador.ConverterParaDto);
    }
}

