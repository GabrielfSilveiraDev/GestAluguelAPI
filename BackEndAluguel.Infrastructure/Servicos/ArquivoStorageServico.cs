using BackEndAluguel.Application.Contratos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementacao do servico de armazenamento de arquivos para contratos.
/// Utiliza o sistema de arquivos local, com pasta configuravel via appsettings.json.
/// </summary>
public class ArquivoStorageServico : IArquivoStorageServico
{
    private readonly string _pastaRaiz;
    private readonly ILogger<ArquivoStorageServico> _logger;

    /// <summary>Inicializa o servico lendo a pasta raiz da configuracao.</summary>
    public ArquivoStorageServico(IConfiguration configuration, ILogger<ArquivoStorageServico> logger)
    {
        _pastaRaiz = configuration["Contratos:PastaArmazenamento"] ?? "wwwroot/contratos";
        _logger = logger;
    }

    /// <summary>
    /// Remove o arquivo fisico do disco. Se nao existir, ignora silenciosamente.
    /// </summary>
    public void DeletarArquivo(string caminhoRelativo)
    {
        var caminhoCompleto = Path.Combine(_pastaRaiz, caminhoRelativo);
        if (File.Exists(caminhoCompleto))
        {
            File.Delete(caminhoCompleto);
            _logger.LogInformation("Arquivo de contrato removido: {Caminho}", caminhoCompleto);
        }
        else
        {
            _logger.LogWarning("Arquivo de contrato nao encontrado para remocao: {Caminho}", caminhoCompleto);
        }
    }
}

