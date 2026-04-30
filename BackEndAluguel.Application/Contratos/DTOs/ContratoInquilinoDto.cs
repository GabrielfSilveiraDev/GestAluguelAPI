namespace BackEndAluguel.Application.Contratos.DTOs;

/// <summary>DTO de leitura dos metadados de um contrato assinado por inquilino.</summary>
public record ContratoInquilinoDto(
    /// <summary>Identificador unico do contrato.</summary>
    Guid Id,
    /// <summary>Identificador do inquilino vinculado.</summary>
    Guid InquilinoId,
    /// <summary>Nome original do arquivo enviado.</summary>
    string NomeOriginalArquivo,
    /// <summary>Tipo MIME do arquivo (application/pdf, image/jpeg, etc.).</summary>
    string TipoConteudo,
    /// <summary>Tamanho do arquivo em bytes.</summary>
    long TamanhoBytes,
    /// <summary>Tamanho formatado para exibicao (ex: "1.2 MB").</summary>
    string TamanhoFormatado,
    /// <summary>Descricao opcional do contrato.</summary>
    string? Descricao,
    /// <summary>Data de upload do arquivo.</summary>
    DateTime CriadoEm,
    /// <summary>Caminho relativo do arquivo no servidor (usado no download).</summary>
    string CaminhoArquivo = ""
);

