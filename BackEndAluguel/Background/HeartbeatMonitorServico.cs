namespace BackEndAluguel.Api.Background;

// Encerra o servidor automaticamente quando nenhum heartbeat é recebido por TempoLimite.
// O frontend envia pings a cada 30s; se o browser fechar, os pings param e o servidor desliga.
public class HeartbeatMonitorServico : BackgroundService
{
    private static readonly TimeSpan TempoLimite = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan IntervaloVerificacao = TimeSpan.FromSeconds(30);

    private readonly HeartbeatEstado _estado;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<HeartbeatMonitorServico> _logger;

    public HeartbeatMonitorServico(
        HeartbeatEstado estado,
        IHostApplicationLifetime lifetime,
        ILogger<HeartbeatMonitorServico> logger)
    {
        _estado = estado;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda o primeiro heartbeat antes de começar a monitorar
        while (!stoppingToken.IsCancellationRequested && _estado.Ultimo is null)
            await Task.Delay(5_000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(IntervaloVerificacao, stoppingToken);

            if (_estado.Ultimo.HasValue &&
                DateTime.UtcNow - _estado.Ultimo.Value > TempoLimite)
            {
                _logger.LogInformation("Nenhum heartbeat por {Seg}s — encerrando servidor.", TempoLimite.TotalSeconds);
                _lifetime.StopApplication();
                return;
            }
        }
    }
}
