using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Application.Contratos.Comandos;
using BackEndAluguel.Application.Contratos.Consultas;
using BackEndAluguel.Application.Contratos.DTOs;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Contratos.Manipuladores;

/// <summary>
/// Manipulador CQRS para registrar os metadados de um contrato enviado pelo usuario.
/// Valida a existencia do inquilino antes de persistir o registro.
/// </summary>
public class RegistrarContratoManipulador : IRequestHandler<RegistrarContratoComando, ContratoInquilinoDto>
{
    private readonly IContratoInquilinoRepositorio _contratoRepositorio;
    private readonly IInquilinoRepositorio _inquilinoRepositorio;

    /// <summary>Inicializa o manipulador com os repositorios necessarios.</summary>
    public RegistrarContratoManipulador(
        IContratoInquilinoRepositorio contratoRepositorio,
        IInquilinoRepositorio inquilinoRepositorio)
    {
        _contratoRepositorio = contratoRepositorio;
        _inquilinoRepositorio = inquilinoRepositorio;
    }

    /// <summary>Processa o registro dos metadados do contrato.</summary>
    public async Task<ContratoInquilinoDto> Handle(RegistrarContratoComando request, CancellationToken cancellationToken)
    {
        // Valida se o inquilino existe antes de registrar o contrato
        var inquilino = await _inquilinoRepositorio.ObterPorIdAsync(request.InquilinoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), request.InquilinoId);

        var contrato = new ContratoInquilino(
            request.InquilinoId,
            request.NomeOriginalArquivo,
            request.CaminhoArquivo,
            request.TipoConteudo,
            request.TamanhoBytes,
            request.Descricao);

        await _contratoRepositorio.AdicionarAsync(contrato, cancellationToken);
        await _contratoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return ConverterParaDto(contrato);
    }

    /// <summary>Converte a entidade ContratoInquilino para o DTO de resposta.</summary>
    internal static ContratoInquilinoDto ConverterParaDto(ContratoInquilino c)
        => new(
            c.Id,
            c.InquilinoId,
            c.NomeOriginalArquivo,
            c.TipoConteudo,
            c.TamanhoBytes,
            FormatarTamanho(c.TamanhoBytes),
            c.Descricao,
            c.CriadoEm,
            c.CaminhoArquivo
        );

    /// <summary>Formata o tamanho do arquivo de forma legivel (KB, MB).</summary>
    private static string FormatarTamanho(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}

/// <summary>Manipulador CQRS para atualizar a descricao de um contrato.</summary>
public class AtualizarDescricaoContratoManipulador : IRequestHandler<AtualizarDescricaoContratoComando, ContratoInquilinoDto>
{
    private readonly IContratoInquilinoRepositorio _repositorio;
    public AtualizarDescricaoContratoManipulador(IContratoInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<ContratoInquilinoDto> Handle(AtualizarDescricaoContratoComando request, CancellationToken cancellationToken)
    {
        var contrato = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(ContratoInquilino), request.Id);

        contrato.AtualizarDescricao(request.Descricao);
        _repositorio.Atualizar(contrato);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return RegistrarContratoManipulador.ConverterParaDto(contrato);
    }
}

/// <summary>
/// Manipulador CQRS para remocao de um contrato.
/// Remove o registro do banco e delega a exclusao do arquivo fisico ao <see cref="IArquivoStorageServico"/>.
/// </summary>
public class RemoverContratoManipulador : IRequestHandler<RemoverContratoComando, bool>
{
    private readonly IContratoInquilinoRepositorio _repositorio;
    private readonly IArquivoStorageServico _arquivoStorage;

    public RemoverContratoManipulador(IContratoInquilinoRepositorio repositorio, IArquivoStorageServico arquivoStorage)
    {
        _repositorio = repositorio;
        _arquivoStorage = arquivoStorage;
    }

    public async Task<bool> Handle(RemoverContratoComando request, CancellationToken cancellationToken)
    {
        var contrato = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(ContratoInquilino), request.Id);

        // Delega a remocao do arquivo fisico para o servico de storage
        _arquivoStorage.DeletarArquivo(contrato.CaminhoArquivo);

        _repositorio.Remover(contrato);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

/// <summary>Manipulador CQRS para obter contrato por ID.</summary>
public class ObterContratoPorIdManipulador : IRequestHandler<ObterContratoPorIdConsulta, ContratoInquilinoDto?>
{
    private readonly IContratoInquilinoRepositorio _repositorio;
    public ObterContratoPorIdManipulador(IContratoInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<ContratoInquilinoDto?> Handle(ObterContratoPorIdConsulta request, CancellationToken cancellationToken)
    {
        var contrato = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return contrato is null ? null : RegistrarContratoManipulador.ConverterParaDto(contrato);
    }
}

/// <summary>Manipulador CQRS para listar contratos por inquilino.</summary>
public class ListarContratosPorInquilinoManipulador : IRequestHandler<ListarContratosPorInquilinoConsulta, IEnumerable<ContratoInquilinoDto>>
{
    private readonly IContratoInquilinoRepositorio _repositorio;
    public ListarContratosPorInquilinoManipulador(IContratoInquilinoRepositorio repositorio) { _repositorio = repositorio; }

    public async Task<IEnumerable<ContratoInquilinoDto>> Handle(ListarContratosPorInquilinoConsulta request, CancellationToken cancellationToken)
    {
        var contratos = await _repositorio.ObterPorInquilinoAsync(request.InquilinoId, cancellationToken);
        return contratos.Select(RegistrarContratoManipulador.ConverterParaDto);
    }
}



