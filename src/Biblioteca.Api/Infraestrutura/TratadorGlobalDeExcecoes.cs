using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Infraestrutura;

/// <summary>
/// Rede de segurança para o que não é falha de negócio.
/// <para>
/// Erros previstos (título inválido, autor inexistente) chegam aos controllers como
/// <c>Result</c> e viram 400/404/409. O que cai aqui é o inesperado — banco fora do ar,
/// bug — e vira 500 com uma mensagem genérica: detalhe de exceção em resposta HTTP
/// é vazamento de informação. O rastro completo vai para o log.
/// </para>
/// </summary>
internal sealed class TratadorGlobalDeExcecoes(
    ILogger<TratadorGlobalDeExcecoes> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Falha não tratada ao processar {Metodo} {Caminho}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro inesperado",
                Detail = "Ocorreu uma falha inesperada ao processar a requisição.",
                Instance = httpContext.Request.Path
            }
        });
    }
}
