using Microsoft.Extensions.DependencyInjection;

namespace BackEndAluguel.Application;

/// <summary>
/// Classe de extensão para registro dos serviços da camada Application no contêiner de DI.
/// Segue o princípio de responsabilidade única (SRP) centralizando o registro dos handlers MediatR.
/// </summary>
public static class ApplicationExtensoes
{
    /// <summary>
    /// Registra todos os manipuladores CQRS (MediatR) da camada Application no contêiner de injeção de dependência.
    /// Deve ser chamado no método de configuração do host da API.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A coleção de serviços para encadeamento (fluent).</returns>
    public static IServiceCollection AdicionarApplication(this IServiceCollection services)
    {
        // Registra automaticamente todos os IRequestHandler da assembly Application
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(ApplicationExtensoes).Assembly));

        return services;
    }
}

