using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato específico de repositório para a entidade <see cref="Inquilino"/>.
/// Estende o repositório genérico com operações específicas para inquilinos.
/// </summary>
public interface IInquilinoRepositorio : IRepositorio<Inquilino>
{
    /// <summary>
    /// Verifica se já existe um inquilino cadastrado com o CPF informado.
    /// Utilizado para evitar duplicatas no cadastro.
    /// </summary>
    /// <param name="cpf">CPF do inquilino (somente dígitos).</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Verdadeiro se já existir um inquilino com este CPF.</returns>
    Task<bool> ExistePorCpfAsync(string cpf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um inquilino pelo CPF.
    /// </summary>
    /// <param name="cpf">CPF do inquilino (somente dígitos).</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>O inquilino encontrado ou nulo.</returns>
    Task<Inquilino?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os inquilinos vinculados a um apartamento específico.
    /// Inclui histórico de inquilinos (não apenas o atual).
    /// </summary>
    /// <param name="apartamentoId">Identificador do apartamento.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista de inquilinos do apartamento.</returns>
    Task<IEnumerable<Inquilino>> ObterPorApartamentoAsync(Guid apartamentoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna os inquilinos cujos contratos vencem dentro do número de dias informado.
    /// Utilizado para geração de alertas de vencimento de contrato.
    /// </summary>
    /// <param name="diasAntecedencia">Quantidade de dias para antecipação do alerta.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>Lista de inquilinos com contrato próximo do vencimento.</returns>
    Task<IEnumerable<Inquilino>> ObterComContratoProximoVencimentoAsync(int diasAntecedencia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca o inquilino ativo de um apartamento com as faturas incluídas.
    /// </summary>
    /// <param name="apartamentoId">Identificador do apartamento.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>O inquilino com as faturas carregadas ou nulo.</returns>
    Task<Inquilino?> ObterComFaturasPorApartamentoAsync(Guid apartamentoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Autentica um inquilino pelo CPF e data de nascimento.
    /// </summary>
    /// <param name="cpf">CPF do inquilino (somente dígitos).</param>
    /// <param name="dataNascimento">Data de nascimento do inquilino.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
    /// <returns>O inquilino correspondente ou nulo se não encontrado.</returns>
    Task<Inquilino?> ObterPorCpfEDataNascimentoAsync(string cpf, DateOnly dataNascimento, CancellationToken cancellationToken = default);
}

