using BackEndAluguel.Application.Auth;
using BackEndAluguel.Application.Comum;
using BackEndAluguel.Application.Contratos;
using BackEndAluguel.Application.Pagamentos;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using BackEndAluguel.Infrastructure.Repositorios;
using BackEndAluguel.Infrastructure.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackEndAluguel.Infrastructure;

/// <summary>
/// Classe de extensão para registro dos serviços da camada Infrastructure no contêiner de DI.
/// Centraliza o registro do DbContext, repositórios e serviços externos seguindo o princípio SRP.
/// </summary>
public static class InfrastructureExtensoes
{
    /// <summary>
    /// Registra o DbContext, os repositórios, serviços externos e demais dependências da camada de Infraestrutura.
    /// </summary>
    public static IServiceCollection AdicionarInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registra o DbContext com SQLite (modo local, sem instalação de servidor)
        services.AddDbContext<AluguelDbContext>(opcoes =>
            opcoes.UseSqlite(
                configuration.GetConnectionString("AluguelDb"),
                sql => sql.MigrationsAssembly(typeof(AluguelDbContext).Assembly.FullName)));

        // Registra os repositorios — seguindo DIP (SOLID)
        services.AddScoped<IApartamentoRepositorio, ApartamentoRepositorio>();
        services.AddScoped<IInquilinoRepositorio, InquilinoRepositorio>();
        services.AddScoped<IFaturaRepositorio, FaturaRepositorio>();
        services.AddScoped<IDependenteRepositorio, DependenteRepositorio>();
        services.AddScoped<IConfiguracaoRepositorio, ConfiguracaoRepositorio>();
        services.AddScoped<IGastoManutencaoRepositorio, GastoManutencaoRepositorio>();
        services.AddScoped<IContratoInquilinoRepositorio, ContratoInquilinoRepositorio>();
        services.AddScoped<IHostRepositorio, HostRepositorio>();

        // Registra o cliente HTTP nomeado para a API do Asaas
        // A URL base e a ApiKey sao lidas do appsettings.json (secao "Asaas")
        services.AddHttpClient("Asaas", (sp, client) =>
        {
            var asaasBaseUrl = configuration["Asaas:BaseUrl"]
                ?? "https://sandbox.asaas.com/api/v3";
            var asaasApiKey = configuration["Asaas:ApiKey"] ?? string.Empty;

            client.BaseAddress = new Uri(asaasBaseUrl);
            // Header de autenticacao exigido pela API do Asaas
            client.DefaultRequestHeaders.Add("access_token", asaasApiKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Registra o servico de gateway de pagamento (implementacao Asaas)
        services.AddScoped<IServicoGatewayPagamento, AsaasServicoGatewayPagamento>();

        // Registra o servico de armazenamento de arquivos (contratos)
        services.AddScoped<IArquivoStorageServico, ArquivoStorageServico>();

        // Registra o servico de geracao de tokens JWT
        services.AddScoped<IJwtServico, JwtServico>();

        // Registra o servico de hashing de senhas (PBKDF2-SHA256)
        services.AddScoped<ISenhaServico, SenhaServico>();

        // Registra o servico de envio de e-mails (SMTP)
        services.AddScoped<IEmailServico, EmailServico>();

        // Registra o gerador de payload PIX (EMV/copia-e-cola, sem API externa)
        services.AddScoped<IPixPayloadGerador, PixPayloadGerador>();

        // Registra o contexto de tenant (multi-tenancy via JWT hostId claim)
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContexto, TenantContexto>();

        return services;
    }
}


