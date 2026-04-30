namespace BackEndAluguel.Application.Comum.Excecoes;

/// <summary>
/// Exceção lançada quando uma entidade não é encontrada no repositório.
/// Utilizada nos manipuladores CQRS para sinalizar ausência de dados.
/// </summary>
public class EntidadeNaoEncontradaExcecao : Exception
{
    /// <summary>
    /// Cria uma nova exceção de entidade não encontrada com uma mensagem descritiva.
    /// </summary>
    /// <param name="nomeEntidade">Nome da entidade que não foi encontrada.</param>
    /// <param name="identificador">Identificador que foi buscado.</param>
    public EntidadeNaoEncontradaExcecao(string nomeEntidade, object identificador)
        : base($"A entidade '{nomeEntidade}' com o identificador '{identificador}' não foi encontrada.")
    {
    }
}

/// <summary>
/// Exceção lançada quando uma regra de negócio é violada durante o processamento de um comando.
/// </summary>
public class RegraDeNegocioExcecao : Exception
{
    /// <summary>
    /// Cria uma nova exceção de regra de negócio com uma mensagem explicativa.
    /// </summary>
    /// <param name="mensagem">Descrição da regra violada.</param>
    public RegraDeNegocioExcecao(string mensagem) : base(mensagem)
    {
    }
}

