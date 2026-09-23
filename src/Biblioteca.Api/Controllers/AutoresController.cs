using Biblioteca.Application.Contratos;
using Biblioteca.Application.Dtos.Autores;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

/// <summary>
/// Endpoints de autor.
/// <para>
/// O controller não contém regra alguma: recebe, delega ao caso de uso e traduz o
/// resultado para HTTP. Toda decisão de negócio está abaixo dele.
/// </para>
/// </summary>
[Route("api/autores")]
public sealed class AutoresController(IAutorAppService autorAppService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AutorResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var autores = await autorAppService.ListarAsync(cancellationToken);

        return Ok(autores);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AutorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await autorAppService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : Ok(resultado.Value);
    }

    [HttpPost]
    [ProducesResponseType<AutorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarAutorRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await autorAppService.CriarAsync(request, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<AutorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarAutorRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await autorAppService.AtualizarAsync(id, request, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : Ok(resultado.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await autorAppService.RemoverAsync(id, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : NoContent();
    }
}
