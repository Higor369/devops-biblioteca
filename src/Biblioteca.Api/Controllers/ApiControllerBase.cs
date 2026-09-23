using Biblioteca.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

/// <summary>
/// Base dos controllers. Concentra a única tradução de domínio para HTTP que existe
/// na aplicação: <see cref="ErrorType"/> vira status code.
/// <para>
/// Sem isto, cada controller repetiria o mesmo <c>switch</c> — e bastaria um esquecer
/// um caso para a API responder 500 onde deveria responder 409.
/// </para>
/// </summary>
// Sem [Produces("application/json")]: ele fixaria o content type de toda resposta,
// inclusive das de erro, que pela RFC 9457 devem sair como application/problem+json.
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Converte uma falha de negócio em uma resposta RFC 9457 (ProblemDetails).</summary>
    protected ObjectResult ProblemaDe(Error erro)
    {
        var statusCode = erro.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problema = new ProblemDetails
        {
            Status = statusCode,
            Title = TituloPara(erro.Type),
            Detail = erro.Message,
            Instance = HttpContext.Request.Path
        };

        // O código estável vai junto para que um cliente possa reagir programaticamente
        // sem depender do texto da mensagem, que pode mudar.
        problema.Extensions["codigo"] = erro.Code;

        var resposta = StatusCode(statusCode, problema);
        resposta.ContentTypes.Add("application/problem+json");

        return resposta;
    }

    private static string TituloPara(ErrorType tipo) => tipo switch
    {
        ErrorType.Validation => "Dados inválidos",
        ErrorType.NotFound => "Recurso não encontrado",
        ErrorType.Conflict => "Conflito com o estado atual",
        _ => "Erro inesperado"
    };
}
