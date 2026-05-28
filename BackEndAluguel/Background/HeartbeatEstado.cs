namespace BackEndAluguel.Api.Background;

public class HeartbeatEstado
{
    private DateTime? _ultimo;

    public void Registrar() => _ultimo = DateTime.UtcNow;

    public DateTime? Ultimo => _ultimo;
}
