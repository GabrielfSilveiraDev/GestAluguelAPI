using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Application.Inquilinos.Comandos;
using BackEndAluguel.Application.Inquilinos.Consultas;
using BackEndAluguel.Application.Inquilinos.DTOs;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Inquilinos.Manipuladores;

public class CriarInquilinoManipulador : IRequestHandler<CriarInquilinoComando, InquilinoDto>
{
    private readonly IInquilinoRepositorio _inquilinoRepositorio;
    private readonly IApartamentoRepositorio _apartamentoRepositorio;

    public CriarInquilinoManipulador(IInquilinoRepositorio inquilinoRepositorio, IApartamentoRepositorio apartamentoRepositorio)
    {
        _inquilinoRepositorio = inquilinoRepositorio;
        _apartamentoRepositorio = apartamentoRepositorio;
    }

    public async Task<InquilinoDto> Handle(CriarInquilinoComando request, CancellationToken cancellationToken)
    {
        var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());

        var cpfJaExiste = await _inquilinoRepositorio.ExistePorCpfAsync(cpfLimpo, cancellationToken);
        if (cpfJaExiste)
            throw new RegraDeNegocioExcecao($"Ja existe um inquilino cadastrado com o CPF '{request.Cpf}'.");

        var apartamento = await _apartamentoRepositorio.ObterPorIdAsync(request.ApartamentoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Apartamento), request.ApartamentoId);

        var inquilino = new Inquilino(
            request.NomeCompleto, request.Cpf, request.QuantidadeMoradores,
            request.DataEntrada, request.DataVencimentoContrato, request.ValorAluguel,
            request.ApartamentoId, request.DataNascimento,
            request.Rg, request.OrgaoEmissor, request.Telefone, request.EstadoCivil,
            request.DiasAlertaVencimento, request.Garagem);

        await _inquilinoRepositorio.AdicionarAsync(inquilino, cancellationToken);
        apartamento.MarcarComoOcupado();
        _apartamentoRepositorio.Atualizar(apartamento);
        await _inquilinoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return ConverterParaDto(inquilino);
    }

    internal static InquilinoDto ConverterParaDto(Inquilino i)
    {
        var estadoCivilDesc = i.EstadoCivil switch
        {
            EstadoCivil.Solteiro => "Solteiro(a)",
            EstadoCivil.Casado => "Casado(a)",
            EstadoCivil.Divorciado => "Divorciado(a)",
            EstadoCivil.Viuvo => "Viúvo(a)",
            EstadoCivil.UniaoEstavel => "União Estável",
            _ => "Não informado"
        };
        return new InquilinoDto(
            i.Id, i.NomeCompleto, i.Cpf, i.DataNascimento,
            i.Rg, i.OrgaoEmissor, i.Telefone, i.EstadoCivil, estadoCivilDesc,
            i.QuantidadeMoradores, i.DataEntrada, i.DataVencimentoContrato,
            i.ValorAluguel, i.Garagem, i.ApartamentoId,
            i.Apartamento?.Numero,
            i.Apartamento?.Bloco,
            i.DiasAlertaVencimento, i.CriadoEm);
    }
}

public class AtualizarInquilinoManipulador : IRequestHandler<AtualizarInquilinoComando, InquilinoDto>
{
    private readonly IInquilinoRepositorio _repositorio;

    public AtualizarInquilinoManipulador(IInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<InquilinoDto> Handle(AtualizarInquilinoComando request, CancellationToken cancellationToken)
    {
        var inquilino = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), request.Id);

        inquilino.Atualizar(request.NomeCompleto, request.QuantidadeMoradores,
            request.DataVencimentoContrato, request.ValorAluguel,
            request.Rg, request.OrgaoEmissor, request.Telefone, request.EstadoCivil,
            request.DiasAlertaVencimento, request.Garagem);

        _repositorio.Atualizar(inquilino);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return CriarInquilinoManipulador.ConverterParaDto(inquilino);
    }
}

public class RemoverInquilinoManipulador : IRequestHandler<RemoverInquilinoComando, bool>
{
    private readonly IInquilinoRepositorio _inquilinoRepositorio;
    private readonly IApartamentoRepositorio _apartamentoRepositorio;

    public RemoverInquilinoManipulador(IInquilinoRepositorio inquilinoRepositorio, IApartamentoRepositorio apartamentoRepositorio)
    {
        _inquilinoRepositorio = inquilinoRepositorio;
        _apartamentoRepositorio = apartamentoRepositorio;
    }

    public async Task<bool> Handle(RemoverInquilinoComando request, CancellationToken cancellationToken)
    {
        var inquilino = await _inquilinoRepositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), request.Id);

        var apartamento = await _apartamentoRepositorio.ObterPorIdAsync(inquilino.ApartamentoId, cancellationToken);
        if (apartamento != null)
        {
            apartamento.MarcarComoDesocupado();
            _apartamentoRepositorio.Atualizar(apartamento);
        }

        _inquilinoRepositorio.Remover(inquilino);
        await _inquilinoRepositorio.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

public class ObterInquilinoPorIdManipulador : IRequestHandler<ObterInquilinoPorIdConsulta, InquilinoDto?>
{
    private readonly IInquilinoRepositorio _repositorio;
    public ObterInquilinoPorIdManipulador(IInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<InquilinoDto?> Handle(ObterInquilinoPorIdConsulta request, CancellationToken cancellationToken)
    {
        var inquilino = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return inquilino is null ? null : CriarInquilinoManipulador.ConverterParaDto(inquilino);
    }
}

public class ListarInquilinosManipulador : IRequestHandler<ListarInquilinosConsulta, IEnumerable<InquilinoDto>>
{
    private readonly IInquilinoRepositorio _repositorio;
    public ListarInquilinosManipulador(IInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<InquilinoDto>> Handle(ListarInquilinosConsulta request, CancellationToken cancellationToken)
    {
        var inquilinos = await _repositorio.ObterTodosAsync(cancellationToken);
        return inquilinos.Select(CriarInquilinoManipulador.ConverterParaDto);
    }
}

public class ListarInquilinosPorApartamentoManipulador : IRequestHandler<ListarInquilinosPorApartamentoConsulta, IEnumerable<InquilinoDto>>
{
    private readonly IInquilinoRepositorio _repositorio;
    public ListarInquilinosPorApartamentoManipulador(IInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<InquilinoDto>> Handle(ListarInquilinosPorApartamentoConsulta request, CancellationToken cancellationToken)
    {
        var inquilinos = await _repositorio.ObterPorApartamentoAsync(request.ApartamentoId, cancellationToken);
        return inquilinos.Select(CriarInquilinoManipulador.ConverterParaDto);
    }
}

public class ListarInquilinosComContratoProximoManipulador : IRequestHandler<ListarInquilinosComContratoProximoConsulta, IEnumerable<InquilinoDto>>
{
    private readonly IInquilinoRepositorio _repositorio;
    public ListarInquilinosComContratoProximoManipulador(IInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<InquilinoDto>> Handle(ListarInquilinosComContratoProximoConsulta request, CancellationToken cancellationToken)
    {
        var inquilinos = await _repositorio.ObterComContratoProximoVencimentoAsync(request.DiasAntecedencia, cancellationToken);
        return inquilinos.Select(CriarInquilinoManipulador.ConverterParaDto);
    }
}

// ────────────────────────────────────────────
// Dependentes
// ────────────────────────────────────────────

public class AdicionarDependenteManipulador : IRequestHandler<AdicionarDependenteComando, DependenteDto>
{
    private readonly IDependenteRepositorio _dependenteRepositorio;
    private readonly IInquilinoRepositorio _inquilinoRepositorio;

    public AdicionarDependenteManipulador(IDependenteRepositorio dependenteRepositorio, IInquilinoRepositorio inquilinoRepositorio)
    {
        _dependenteRepositorio = dependenteRepositorio;
        _inquilinoRepositorio = inquilinoRepositorio;
    }

    public async Task<DependenteDto> Handle(AdicionarDependenteComando request, CancellationToken cancellationToken)
    {
        var inquilino = await _inquilinoRepositorio.ObterPorIdAsync(request.InquilinoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), request.InquilinoId);

        var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());
        if (await _dependenteRepositorio.ExistePorCpfAsync(cpfLimpo, cancellationToken))
            throw new RegraDeNegocioExcecao($"Ja existe um dependente cadastrado com o CPF '{request.Cpf}'.");

        var dependente = new Dependente(request.NomeCompleto, request.Cpf, request.Rg,
            request.OrgaoEmissor, request.DataNascimento, request.Telefone,
            request.EstadoCivil, request.InquilinoId);

        await _dependenteRepositorio.AdicionarAsync(dependente, cancellationToken);
        await _dependenteRepositorio.SalvarAlteracoesAsync(cancellationToken);
        return ConverterDependenteParaDto(dependente);
    }

    internal static DependenteDto ConverterDependenteParaDto(Dependente d)
    {
        var estadoCivilDesc = d.EstadoCivil switch
        {
            EstadoCivil.Solteiro => "Solteiro(a)",
            EstadoCivil.Casado => "Casado(a)",
            EstadoCivil.Divorciado => "Divorciado(a)",
            EstadoCivil.Viuvo => "Viuvo(a)",
            EstadoCivil.UniaoEstavel => "Uniao Estavel",
            _ => "Desconhecido"
        };
        return new DependenteDto(d.Id, d.NomeCompleto, d.Cpf, d.Rg, d.OrgaoEmissor,
            d.DataNascimento, d.Telefone, d.EstadoCivil, estadoCivilDesc, d.InquilinoId, d.CriadoEm);
    }
}

public class AtualizarDependenteManipulador : IRequestHandler<AtualizarDependenteComando, DependenteDto>
{
    private readonly IDependenteRepositorio _repositorio;
    public AtualizarDependenteManipulador(IDependenteRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<DependenteDto> Handle(AtualizarDependenteComando request, CancellationToken cancellationToken)
    {
        var dependente = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Dependente), request.Id);

        dependente.Atualizar(request.NomeCompleto, request.Rg, request.OrgaoEmissor,
            request.DataNascimento, request.Telefone, request.EstadoCivil);

        _repositorio.Atualizar(dependente);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return AdicionarDependenteManipulador.ConverterDependenteParaDto(dependente);
    }
}

public class RemoverDependenteManipulador : IRequestHandler<RemoverDependenteComando, bool>
{
    private readonly IDependenteRepositorio _repositorio;
    public RemoverDependenteManipulador(IDependenteRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<bool> Handle(RemoverDependenteComando request, CancellationToken cancellationToken)
    {
        var dependente = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Dependente), request.Id);

        _repositorio.Remover(dependente);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

public class ListarDependentesPorInquilinoManipulador : IRequestHandler<ListarDependentesPorInquilinoConsulta, IEnumerable<DependenteDto>>
{
    private readonly IDependenteRepositorio _repositorio;
    public ListarDependentesPorInquilinoManipulador(IDependenteRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<DependenteDto>> Handle(ListarDependentesPorInquilinoConsulta request, CancellationToken cancellationToken)
    {
        var dependentes = await _repositorio.ObterPorInquilinoAsync(request.InquilinoId, cancellationToken);
        return dependentes.Select(AdicionarDependenteManipulador.ConverterDependenteParaDto);
    }
}

public class ObterDependentePorIdManipulador : IRequestHandler<ObterDependentePorIdConsulta, DependenteDto?>
{
    private readonly IDependenteRepositorio _repositorio;
    public ObterDependentePorIdManipulador(IDependenteRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<DependenteDto?> Handle(ObterDependentePorIdConsulta request, CancellationToken cancellationToken)
    {
        var dependente = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return dependente is null ? null : AdicionarDependenteManipulador.ConverterDependenteParaDto(dependente);
    }
}
