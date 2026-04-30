using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Infrastructure.Repositorios;

/// <summary>
/// Implementação concreta do repositório de <see cref="Inquilino"/>.
/// Herda as operações genéricas do <see cref="RepositorioBase{T}"/> e adiciona
/// queries específicas para inquilinos.
/// </summary>
public class InquilinoRepositorio : RepositorioBase<Inquilino>, IInquilinoRepositorio
{
    /// <summary>
    /// Inicializa o repositório com o contexto do banco de dados.
    /// </summary>
    public InquilinoRepositorio(AluguelDbContext contexto) : base(contexto)
    {
    }

    /// <summary>
    /// Busca um inquilino pelo ID incluindo o Apartamento vinculado.
    /// Sobrescreve o método base para garantir que a propriedade de navegação seja carregada.
    /// </summary>
    public override async Task<Inquilino?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(i => i.Apartamento)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    /// <summary>
    /// Verifica se já existe um inquilino cadastrado com o CPF informado.
    /// </summary>
    public async Task<bool> ExistePorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(i => i.Cpf == cpf, cancellationToken);

    /// <summary>
    /// Busca um inquilino pelo CPF.
    /// </summary>
    public async Task<Inquilino?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(i => i.Cpf == cpf, cancellationToken);

    /// <summary>
    /// Retorna todos os inquilinos vinculados a um apartamento específico.
    /// </summary>
    public async Task<IEnumerable<Inquilino>> ObterPorApartamentoAsync(Guid apartamentoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(i => i.ApartamentoId == apartamentoId)
            .OrderByDescending(i => i.DataEntrada)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retorna os inquilinos cujos contratos vencem dentro do número de dias informado.
    /// </summary>
    public async Task<IEnumerable<Inquilino>> ObterComContratoProximoVencimentoAsync(
        int diasAntecedencia, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var dataLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(diasAntecedencia));

        return await DbSet.AsNoTracking()
            .Where(i => i.DataVencimentoContrato >= hoje && i.DataVencimentoContrato <= dataLimite)
            .OrderBy(i => i.DataVencimentoContrato)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca o inquilino ativo de um apartamento com as faturas carregadas via Include.
    /// </summary>
    public async Task<Inquilino?> ObterComFaturasPorApartamentoAsync(
        Guid apartamentoId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(i => i.Faturas)
            .FirstOrDefaultAsync(i => i.ApartamentoId == apartamentoId, cancellationToken);

    /// <summary>
    /// Autentica inquilino pelo CPF e data de nascimento (segundo fator para login).
    /// </summary>
    public async Task<Inquilino?> ObterPorCpfEDataNascimentoAsync(
        string cpf, DateOnly dataNascimento, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            i => i.Cpf == cpf && i.DataNascimento == dataNascimento,
            cancellationToken);
}

