using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

public class ConfiguracaoRepositorio : RepositorioBase<Configuracao>, IConfiguracaoRepositorio
{
    public ConfiguracaoRepositorio(AluguelDbContext contexto) : base(contexto) { }

    public async Task<Configuracao?> ObterConfiguracaoAsync(CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(cancellationToken);
}

