using Biblioteca.Application.Contratos;
using Biblioteca.Application.Dtos.Livros;
using Biblioteca.Application.Mapeamentos;
using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Erros;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;

namespace Biblioteca.Application.Servicos;

/// <inheritdoc cref="ILivroAppService"/>
public sealed class LivroAppService(
    ILivroRepository livroRepository,
    IAutorRepository autorRepository,
    IGeneroRepository generoRepository,
    ILivroDomainService livroDomainService,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : ILivroAppService
{
    public async Task<Result<LivroResponse>> CriarAsync(
        CriarLivroRequest request,
        CancellationToken cancellationToken = default)
    {
        var livroCriado = Livro.Criar(
            request.Titulo,
            request.AnoDePublicacao,
            request.AutorId,
            request.GeneroId,
            relogio);

        if (livroCriado.IsFailure)
        {
            return Result.Failure<LivroResponse>(livroCriado.Error);
        }

        // Só depois de o livro passar na validação própria vale a pena ir ao banco
        // conferir se o autor e o gênero informados existem de fato.
        var referenciasValidas = await livroDomainService.GarantirReferenciasValidasAsync(
            request.AutorId,
            request.GeneroId,
            cancellationToken);

        if (referenciasValidas.IsFailure)
        {
            return Result.Failure<LivroResponse>(referenciasValidas.Error);
        }

        await livroRepository.AdicionarAsync(livroCriado.Value, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return await MontarRespostaAsync(livroCriado.Value, cancellationToken);
    }

    public async Task<Result<LivroResponse>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var livro = await livroRepository.ObterPorIdAsync(id, cancellationToken);

        return livro is null
            ? Result.Failure<LivroResponse>(ErrosDeLivro.NaoEncontrado(id))
            : await MontarRespostaAsync(livro, cancellationToken);
    }

    public async Task<IReadOnlyList<LivroResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var livros = await livroRepository.ListarAsync(cancellationToken);
        if (livros.Count == 0)
        {
            return [];
        }

        // Três consultas no total, independentemente de quantos livros existam:
        // uma de livros, uma de autores e uma de gêneros. Buscar autor e gênero
        // dentro do laço seria o clássico problema de N+1.
        var autores = await autorRepository.ListarPorIdsAsync(
            [.. livros.Select(livro => livro.AutorId).Distinct()],
            cancellationToken);

        var generos = await generoRepository.ListarPorIdsAsync(
            [.. livros.Select(livro => livro.GeneroId).Distinct()],
            cancellationToken);

        var autoresPorId = autores.ToDictionary(autor => autor.Id);
        var generosPorId = generos.ToDictionary(genero => genero.Id);

        return
        [
            .. livros.Select(livro => livro.ParaResponse(
                autoresPorId[livro.AutorId],
                generosPorId[livro.GeneroId]))
        ];
    }

    public async Task<Result<LivroResponse>> AtualizarAsync(
        Guid id,
        AtualizarLivroRequest request,
        CancellationToken cancellationToken = default)
    {
        var livro = await livroRepository.ObterPorIdAsync(id, cancellationToken);
        if (livro is null)
        {
            return Result.Failure<LivroResponse>(ErrosDeLivro.NaoEncontrado(id));
        }

        var livroAtualizado = livro.Atualizar(
            request.Titulo,
            request.AnoDePublicacao,
            request.AutorId,
            request.GeneroId,
            relogio);

        if (livroAtualizado.IsFailure)
        {
            return Result.Failure<LivroResponse>(livroAtualizado.Error);
        }

        var referenciasValidas = await livroDomainService.GarantirReferenciasValidasAsync(
            request.AutorId,
            request.GeneroId,
            cancellationToken);

        if (referenciasValidas.IsFailure)
        {
            return Result.Failure<LivroResponse>(referenciasValidas.Error);
        }

        livroRepository.Atualizar(livro);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return await MontarRespostaAsync(livro, cancellationToken);
    }

    public async Task<Result> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var livro = await livroRepository.ObterPorIdAsync(id, cancellationToken);
        if (livro is null)
        {
            return Result.Failure(ErrosDeLivro.NaoEncontrado(id));
        }

        // Excluir livro não tem regra de negócio: nada depende dele.
        livroRepository.Remover(livro);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Carrega o autor e o gênero do livro para montar a resposta expandida.
    /// </summary>
    private async Task<Result<LivroResponse>> MontarRespostaAsync(
        Livro livro,
        CancellationToken cancellationToken)
    {
        var autor = await autorRepository.ObterPorIdAsync(livro.AutorId, cancellationToken);
        var genero = await generoRepository.ObterPorIdAsync(livro.GeneroId, cancellationToken);

        // Chegar aqui com nulo significaria que a chave estrangeira do banco foi violada:
        // é defeito, não falha de negócio prevista.
        if (autor is null || genero is null)
        {
            throw new InvalidOperationException(
                $"O livro {livro.Id} aponta para autor ou gênero inexistente.");
        }

        return Result.Success(livro.ParaResponse(autor, genero));
    }
}
