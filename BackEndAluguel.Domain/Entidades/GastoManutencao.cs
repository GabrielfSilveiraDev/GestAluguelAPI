namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Representa um gasto de manutencao vinculado a um apartamento.
/// </summary>
public class GastoManutencao : EntidadeBase
{
    public Guid ApartamentoId { get; private set; }
    public Apartamento? Apartamento { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }
    public string? Observacao { get; private set; }

    protected GastoManutencao() { }

    public GastoManutencao(Guid apartamentoId, string descricao, decimal valor, DateOnly data, string? observacao = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descricao do gasto nao pode ser vazia.", nameof(descricao));
        if (valor <= 0)
            throw new ArgumentException("O valor do gasto deve ser maior que zero.", nameof(valor));

        ApartamentoId = apartamentoId;
        Descricao = descricao.Trim();
        Valor = valor;
        Data = data;
        Observacao = observacao?.Trim();
    }

    public void Atualizar(string descricao, decimal valor, DateOnly data, string? observacao = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descricao do gasto nao pode ser vazia.", nameof(descricao));
        if (valor <= 0)
            throw new ArgumentException("O valor do gasto deve ser maior que zero.", nameof(valor));

        Descricao = descricao.Trim();
        Valor = valor;
        Data = data;
        Observacao = observacao?.Trim();
        MarcarComoAtualizado();
    }
}

