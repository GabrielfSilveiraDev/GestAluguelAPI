using BackEndAluguel.Application.Faturas.Comandos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackEndAluguel.Api.Background;

/// <summary>
/// Servico de background que verifica automaticamente as faturas vencidas
/// e atualiza o status para "Atrasado" uma vez por dia.
///
/// Por que IHostedService/BackgroundService?
/// O status de faturas atrasadas precisa ser atualizado mesmo sem interacao do usuario.
/// Este servico roda em background no ciclo de vida da aplicacao, verificando diariamente
/// se existem faturas com data de vencimento ultrapassada e status ainda "Pendente".
/// Usa IServiceScopeFactory pois os repositorios/handlers sao Scoped e o BackgroundService e Singleton.
/// </summary>
public class VerificarFaturasVencidasServico : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VerificarFaturasVencidasServico> _logger;

    // Intervalo de verificacao: uma vez por dia
    private static readonly TimeSpan IntervaloVerificacao = TimeSpan.FromHours(24);

    /// <summary>Inicializa o servico de background com a fabrica de escopos e o logger.</summary>
    public VerificarFaturasVencidasServico(
        IServiceScopeFactory scopeFactory,
        ILogger<VerificarFaturasVencidasServico> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executa o loop de verificacao de faturas vencidas.
    /// Ao iniciar, executa imediatamente e depois repete a cada 24 horas.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servico de verificacao de faturas vencidas iniciado.");

        // Executa imediatamente ao iniciar a aplicacao
        await ProcessarFaturasVencidasAsync(stoppingToken);

        // Aguarda e repete diariamente
        using var timer = new PeriodicTimer(IntervaloVerificacao);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessarFaturasVencidasAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Cria um escopo de DI e dispara o comando de processamento de faturas vencidas via MediatR.
    /// O escopo garante que os repositorios Scoped sejam instanciados corretamente.
    /// </summary>
    private async Task ProcessarFaturasVencidasAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Cria um novo escopo para resolver servicos Scoped
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var quantidade = await mediator.Send(new ProcessarFaturasVencidasComando(), cancellationToken);

            if (quantidade > 0)
                _logger.LogInformation(
                    "Verificacao de faturas vencidas concluida: {Quantidade} fatura(s) marcada(s) como atrasada(s).",
                    quantidade);
            else
                _logger.LogDebug("Verificacao de faturas vencidas: nenhuma fatura nova atrasada encontrada.");
        }
        catch (OperationCanceledException)
        {
            // Cancelamento esperado durante o shutdown — nao e um erro
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar faturas vencidas no servico de background.");
        }
    }
}

