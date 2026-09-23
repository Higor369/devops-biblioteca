using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Infraestrutura;

/// <summary>
/// Alinha a resposta das <c>DataAnnotations</c> ao mesmo contrato de erro usado pelas
/// falhas de negócio.
/// <para>
/// Sem isto a API responderia 400 em dois formatos diferentes: um vindo do
/// <c>[ApiController]</c>, quando uma anotação falha, e outro vindo do domínio. Quem
/// consome a API teria de saber lidar com os dois.
/// </para>
/// </summary>
internal static class RespostaDeValidacao
{
    public static IServiceCollection AddRespostaDeValidacaoPadronizada(this IServiceCollection services) =>
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = contexto =>
            {
                var problema = new ValidationProblemDetails(contexto.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Dados inválidos",
                    Instance = contexto.HttpContext.Request.Path
                };

                // Mesmo campo extra que as falhas de negócio carregam. Aqui o código é
                // sempre o mesmo, porque o detalhe de qual campo falhou já vai em "errors".
                problema.Extensions["codigo"] = "requisicao.invalida";

                return new BadRequestObjectResult(problema)
                {
                    ContentTypes = { "application/problem+json" }
                };
            });
}
