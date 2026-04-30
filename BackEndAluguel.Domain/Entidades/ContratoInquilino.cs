namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Representa o arquivo de contrato assinado vinculado a um inquilino.
/// Armazena os metadados do arquivo (PDF ou imagem) enviado pelo administrador.
/// O conteudo binario e salvo no sistema de arquivos; aqui apenas os metadados.
/// </summary>
public class ContratoInquilino : EntidadeBase
{
    /// <summary>Identificador do inquilino ao qual o contrato pertence.</summary>
    public Guid InquilinoId { get; private set; }

    /// <summary>Propriedade de navegacao para o inquilino vinculado.</summary>
    public Inquilino? Inquilino { get; private set; }

    /// <summary>Nome original do arquivo enviado pelo usuario.</summary>
    public string NomeOriginalArquivo { get; private set; } = string.Empty;

    /// <summary>
    /// Caminho relativo do arquivo salvo no servidor (ex: "contratos/guid-nome.pdf").
    /// E composto pelo administrador no momento do upload.
    /// </summary>
    public string CaminhoArquivo { get; private set; } = string.Empty;

    /// <summary>Tipo MIME do arquivo (ex: "application/pdf", "image/jpeg").</summary>
    public string TipoConteudo { get; private set; } = string.Empty;

    /// <summary>Tamanho do arquivo em bytes.</summary>
    public long TamanhoBytes { get; private set; }

    /// <summary>Descricao opcional do contrato (ex: "Contrato de locacao 2025").</summary>
    public string? Descricao { get; private set; }

    /// <summary>Construtor protegido requerido pelo Entity Framework Core.</summary>
    protected ContratoInquilino() { }

    /// <summary>
    /// Cria um novo registro de contrato vinculado ao inquilino.
    /// </summary>
    /// <param name="inquilinoId">Identificador do inquilino.</param>
    /// <param name="nomeOriginalArquivo">Nome original do arquivo enviado.</param>
    /// <param name="caminhoArquivo">Caminho relativo no servidor onde o arquivo esta salvo.</param>
    /// <param name="tipoConteudo">Tipo MIME do arquivo.</param>
    /// <param name="tamanhoBytes">Tamanho do arquivo em bytes.</param>
    /// <param name="descricao">Descricao opcional do contrato.</param>
    public ContratoInquilino(
        Guid inquilinoId,
        string nomeOriginalArquivo,
        string caminhoArquivo,
        string tipoConteudo,
        long tamanhoBytes,
        string? descricao = null)
    {
        if (inquilinoId == Guid.Empty)
            throw new ArgumentException("O ID do inquilino nao pode ser vazio.", nameof(inquilinoId));
        if (string.IsNullOrWhiteSpace(nomeOriginalArquivo))
            throw new ArgumentException("O nome do arquivo nao pode ser vazio.", nameof(nomeOriginalArquivo));
        if (string.IsNullOrWhiteSpace(caminhoArquivo))
            throw new ArgumentException("O caminho do arquivo nao pode ser vazio.", nameof(caminhoArquivo));
        if (string.IsNullOrWhiteSpace(tipoConteudo))
            throw new ArgumentException("O tipo de conteudo nao pode ser vazio.", nameof(tipoConteudo));
        if (tamanhoBytes <= 0)
            throw new ArgumentException("O tamanho do arquivo deve ser maior que zero.", nameof(tamanhoBytes));

        InquilinoId = inquilinoId;
        NomeOriginalArquivo = nomeOriginalArquivo.Trim();
        CaminhoArquivo = caminhoArquivo.Trim();
        TipoConteudo = tipoConteudo.Trim();
        TamanhoBytes = tamanhoBytes;
        Descricao = descricao?.Trim();
    }

    /// <summary>Atualiza a descricao do contrato.</summary>
    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao?.Trim();
        MarcarComoAtualizado();
    }
}

