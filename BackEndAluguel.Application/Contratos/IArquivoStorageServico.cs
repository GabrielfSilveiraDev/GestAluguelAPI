namespace BackEndAluguel.Application.Contratos;

/// <summary>
/// Abstracao para operacoes de armazenamento de arquivos de contrato.
/// Seguindo o principio de inversao de dependencia (DIP - SOLID), a camada Application
/// depende desta interface; a implementacao concreta fica na camada Infrastructure/API.
/// </summary>
public interface IArquivoStorageServico
{
    /// <summary>
    /// Remove um arquivo do armazenamento pelo caminho relativo.
    /// Se o arquivo nao existir, a operacao e ignorada silenciosamente.
    /// </summary>
    /// <param name="caminhoRelativo">Caminho relativo do arquivo armazenado.</param>
    void DeletarArquivo(string caminhoRelativo);
}

