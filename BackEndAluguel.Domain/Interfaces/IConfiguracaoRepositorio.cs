using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

public interface IConfiguracaoRepositorio : IRepositorio<Configuracao>
{
    /// <summary>Retorna a configuracao global unica do sistema.</summary>
    Task<Configuracao?> ObterConfiguracaoAsync(CancellationToken cancellationToken = default);
}

