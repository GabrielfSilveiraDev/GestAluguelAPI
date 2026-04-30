namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Entidade que representa um apartamento dentro do condomínio.
/// </summary>
public class Apartamento : EntidadeBase
{
    /// <summary>
    /// Número do apartamento (ex: 101, 202, 303).
    /// </summary>
    public string Numero { get; private set; } = string.Empty;

    /// <summary>
    /// Bloco/Torre ao qual o apartamento pertence. Opcional.
    /// </summary>
    public string Bloco { get; private set; } = string.Empty;

    /// <summary>
    /// Indica se o apartamento está atualmente ocupado por algum inquilino.
    /// </summary>
    public bool Ocupado { get; private set; }

    // =============================================================
    // RELACIONAMENTO: 1 Apartamento -> N Inquilinos
    // Um apartamento pode ser alugado por diversos inquilinos
    // ao longo do tempo (histórico), mas somente um pode estar
    // ativo (com contrato vigente) em determinado momento.
    // =============================================================

    /// <summary>
    /// Lista de inquilinos que já ocuparam ou ocupam o apartamento.
    /// Representa o relacionamento Um-para-Muitos (1:N) entre Apartamento e Inquilino.
    /// </summary>
    public ICollection<Inquilino> Inquilinos { get; private set; } = new List<Inquilino>();

    /// <summary>
    /// Gastos de manutenção do apartamento.
    /// </summary>
    public ICollection<GastoManutencao> GastosManutencao { get; private set; } = new List<GastoManutencao>();

    /// <summary>
    /// Construtor protegido requerido pelo Entity Framework Core.
    /// </summary>
    protected Apartamento() { }

    /// <summary>
    /// Cria uma nova instância de Apartamento com número e bloco definidos.
    /// </summary>
    /// <param name="numero">Número do apartamento.</param>
    /// <param name="bloco">Bloco ou torre do apartamento. Opcional.</param>
    /// <exception cref="ArgumentException">Lançado quando número estiver vazio.</exception>
    public Apartamento(string numero, string? bloco = null)
    {
        ValidarNumero(numero);
        Numero = numero.Trim().ToUpper();
        Bloco = string.IsNullOrWhiteSpace(bloco) ? string.Empty : bloco.Trim().ToUpper();
        Ocupado = false;
    }

    /// <summary>
    /// Atualiza o número e/ou bloco do apartamento.
    /// </summary>
    /// <param name="numero">Novo número do apartamento.</param>
    /// <param name="bloco">Novo bloco do apartamento. Opcional.</param>
    public void Atualizar(string numero, string? bloco = null)
    {
        ValidarNumero(numero);
        Numero = numero.Trim().ToUpper();
        Bloco = string.IsNullOrWhiteSpace(bloco) ? string.Empty : bloco.Trim().ToUpper();
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Marca o apartamento como ocupado quando um inquilino é vinculado.
    /// </summary>
    public void MarcarComoOcupado()
    {
        Ocupado = true;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Marca o apartamento como desocupado quando o inquilino encerra o contrato.
    /// </summary>
    public void MarcarComoDesocupado()
    {
        Ocupado = false;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Valida que o número do apartamento não está vazio.
    /// </summary>
    private static void ValidarNumero(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("O número do apartamento não pode ser vazio.", nameof(numero));
    }
}
