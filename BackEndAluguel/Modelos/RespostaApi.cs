namespace BackEndAluguel.Api.Modelos;
/// <summary>
/// Modelo padronizado de resposta da API para operacoes bem-sucedidas.
/// Garante consistencia no formato JSON retornado em todos os endpoints.
/// </summary>
/// <typeparam name="T">Tipo do dado retornado no campo <c>dados</c>.</typeparam>
public class RespostaApi<T>
{
    /// <summary>Indica se a operacao foi concluida com sucesso.</summary>
    public bool Sucesso { get; init; }
    /// <summary>Mensagem descritiva sobre o resultado da operacao.</summary>
    public string Mensagem { get; init; } = string.Empty;
    /// <summary>Dados retornados pela operacao. Nulo em caso de erro.</summary>
    public T? Dados { get; init; }
    /// <summary>
    /// Cria uma resposta de sucesso com dados e mensagem opcional.
    /// </summary>
    public static RespostaApi<T> Ok(T dados, string mensagem = "Operacao realizada com sucesso.")
        => new() { Sucesso = true, Mensagem = mensagem, Dados = dados };
    /// <summary>
    /// Cria uma resposta de sucesso sem dados (ex: para DELETE).
    /// </summary>
    public static RespostaApi<T> OkSemDados(string mensagem = "Operacao realizada com sucesso.")
        => new() { Sucesso = true, Mensagem = mensagem };
}
/// <summary>
/// Modelo padronizado de resposta de erro da API.
/// Retornado pelo middleware global de tratamento de excecoes.
/// </summary>
public class RespostaErro
{
    /// <summary>Sempre falso nas respostas de erro.</summary>
    public bool Sucesso => false;
    /// <summary>Mensagem principal descrevendo o erro ocorrido.</summary>
    public string Mensagem { get; init; } = string.Empty;
    /// <summary>
    /// Lista de erros detalhados (validacoes, multiplos problemas).
    /// Vazio quando ha apenas um erro principal.
    /// </summary>
    public IEnumerable<string> Erros { get; init; } = Enumerable.Empty<string>();
    /// <summary>
    /// Cria uma resposta de erro com mensagem unica.
    /// </summary>
    public static RespostaErro Criar(string mensagem)
        => new() { Mensagem = mensagem };
    /// <summary>
    /// Cria uma resposta de erro com multiplos erros detalhados.
    /// </summary>
    public static RespostaErro CriarComErros(string mensagem, IEnumerable<string> erros)
        => new() { Mensagem = mensagem, Erros = erros };
}