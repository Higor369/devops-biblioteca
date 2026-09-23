using Biblioteca.Application.Contratos;
using Biblioteca.Application.Dtos.Livros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

/// <inheritdoc cref="AutoresController"/>
[Route("api/livros")]
public sealed class LivrosController(ILivroAppService livroAppService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LivroResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var livros = await livroAppService.ListarAsync(cancellationToken);

        return Ok(livros);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await livroAppService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : Ok(resultado.Value);
    }

    /// <remarks>
    /// Responde 404 quando o autor ou o gênero informados não existem: o recurso
    /// referenciado é que está ausente, não a requisição que está malformada.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarLivroRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await livroAppService.CriarAsync(request, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarLivroRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await livroAppService.AtualizarAsync(id, request, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : Ok(resultado.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await livroAppService.RemoverAsync(id, cancellationToken);

        return resultado.IsFailure
            ? ProblemaDe(resultado.Error)
            : NoContent();
    }
}
