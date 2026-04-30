using System.Net;
using System.Text.Json;
using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Comum.Excecoes;

namespace BackEndAluguel.Api.Middleware;

/// <summary>
/// Middleware global de tratamento de exceções.
/// Intercepta todas as exceções não tratadas da aplicação e retorna
/// respostas HTTP padronizadas com o modelo <see cref="RespostaErro"/>.
///
/// Mapeamento de exceções para HTTP status codes:
/// - <see cref="EntidadeNaoEncontradaExcecao"/> → 404 Not Found
/// - <see cref="RegraDeNegocioExcecao"/>        → 400 Bad Request
/// - <see cref="ArgumentException"/>            → 400 Bad Request
/// - <see cref="InvalidOperationException"/>    → 422 Unprocessable Entity
/// - <see cref="Exception"/> (outros)           → 500 Internal Server Error
/// </summary>
public class TratamentoDeErrosMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeErrosMiddleware> _logger;

    private static readonly JsonSerializerOptions _opcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Inicializa o middleware com o próximo delegate e o logger.
    /// </summary>
    public TratamentoDeErrosMiddleware(RequestDelegate proximo, ILogger<TratamentoDeErrosMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    /// <summary>
    /// Executa o middleware: chama o próximo componente do pipeline e trata qualquer exceção lançada.
    /// </summary>
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (Exception excecao)
        {
            await TratarExcecaoAsync(contexto, excecao);
        }
    }

    /// <summary>
    /// Mapeia a exceção para o código HTTP e a resposta JSON padronizada.
    /// </summary>
    private async Task TratarExcecaoAsync(HttpContext contexto, Exception excecao)
    {
        var (statusCode, resposta) = excecao switch
        {
            EntidadeNaoEncontradaExcecao ex => (
                HttpStatusCode.NotFound,
                RespostaErro.Criar(ex.Message)),

            RegraDeNegocioExcecao ex => (
                HttpStatusCode.BadRequest,
                RespostaErro.Criar(ex.Message)),

            ArgumentException ex => (
                HttpStatusCode.BadRequest,
                RespostaErro.Criar(ex.Message)),

            InvalidOperationException ex => (
                HttpStatusCode.UnprocessableEntity,
                RespostaErro.Criar(ex.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                RespostaErro.Criar("Ocorreu um erro interno no servidor. Tente novamente mais tarde."))
        };

        // Log completo apenas para erros não esperados (500)
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(excecao, "Erro interno não tratado: {Mensagem}", excecao.Message);
        else
            _logger.LogWarning("Erro de negócio [{Status}]: {Mensagem}", (int)statusCode, excecao.Message);

        contexto.Response.ContentType = "application/json";
        contexto.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(resposta, _opcoesJson);
        await contexto.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extensão para registrar o middleware de tratamento de erros no pipeline do ASP.NET Core.
/// </summary>
public static class TratamentoDeErrosExtensao
{
    /// <summary>
    /// Adiciona o <see cref="TratamentoDeErrosMiddleware"/> ao pipeline de requisições HTTP.
    /// Deve ser registrado como o primeiro middleware para capturar todas as exceções.
    /// </summary>
    public static IApplicationBuilder UseTratamentoDeErros(this IApplicationBuilder app)
        => app.UseMiddleware<TratamentoDeErrosMiddleware>();
}

