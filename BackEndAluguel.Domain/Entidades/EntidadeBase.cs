namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Classe base abstrata para todas as entidades do domínio.
/// Garante que toda entidade possua um identificador único (Guid),
/// bem como as datas de criação e última atualização para auditoria.
/// </summary>
public abstract class EntidadeBase
{
    /// <summary>
    /// Identificador único da entidade, gerado automaticamente como Guid.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Data e hora em que a entidade foi criada no sistema.
    /// </summary>
    public DateTime CriadoEm { get; private set; }

    /// <summary>
    /// Data e hora da última atualização da entidade.
    /// Pode ser nulo caso a entidade nunca tenha sido modificada após a criação.
    /// </summary>
    public DateTime? AtualizadoEm { get; private set; }

    /// <summary>
    /// Construtor protegido que inicializa o identificador único e a data de criação.
    /// </summary>
    protected EntidadeBase()
    {
        Id = Guid.NewGuid();
        CriadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza a data de última modificação para o momento atual (UTC).
    /// Deve ser chamado toda vez que alguma propriedade da entidade for alterada.
    /// </summary>
    protected void MarcarComoAtualizado()
    {
        AtualizadoEm = DateTime.UtcNow;
    }
}

