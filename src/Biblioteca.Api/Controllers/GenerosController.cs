using Biblioteca.Application.Contratos;
using Biblioteca.Application.Dtos.Generos;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

/// <inheritdoc cref="AutoresController"/>
[Route("api/generos")]
public sealed class GenerosController(IGeneroAppService generoAppService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GeneroResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var generos = await generoAppService.ListarAsync(cancellationToken);

        return Ok(generos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GeneroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await generoAppService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : Ok(resultado.Value);
    }

    [HttpPost]
    [ProducesResponseType<GeneroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarGeneroRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await generoAppService.CriarAsync(request, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<GeneroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarGeneroRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await generoAppService.AtualizarAsync(id, request, cancellationToken);

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
        var resultado = await generoAppService.RemoverAsync(id, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : NoContent();
    }
}
