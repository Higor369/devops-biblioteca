using Biblioteca.Application.Contratos;
using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Mapeamentos;
using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Erros;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;

namespace Biblioteca.Application.Servicos;

/// <inheritdoc cref="IAutorAppService"/>
public sealed class AutorAppService(
    IAutorRepository autorRepository,
    IAutorDomainService autorDomainService,
    IUnitOfWork unitOfWork) : IAutorAppService
{
    public async Task<Result<AutorResponse>> CriarAsync(
        CriarAutorRequest request,
        CancellationToken cancellationToken = default)
    {
        var autorCriado = Autor.Criar(request.Nome);
        if (autorCriado.IsFailure)
        {
            return Result.Failure<AutorResponse>(autorCriado.Error);
        }

        await autorRepository.AdicionarAsync(autorCriado.Value, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success(autorCriado.Value.ParaResponse());
    }

    public async Task<Result<AutorResponse>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var autor = await autorRepository.ObterPorIdAsync(id, cancellationToken);

        return autor is null
            ? Result.Failure<AutorResponse>(ErrosDeAutor.NaoEncontrado(id))
            : Result.Success(autor.ParaResponse());
    }

    public async Task<IReadOnlyList<AutorResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var autores = await autorRepository.ListarAsync(cancellationToken);

        return autores.ParaResponse();
    }

    public async Task<Result<AutorResponse>> AtualizarAsync(
        Guid id,
        AtualizarAutorRequest request,
        CancellationToken cancellationToken = default)
    {
        var autor = await autorRepository.ObterPorIdAsync(id, cancellationToken);
        if (autor is null)
        {
            return Result.Failure<AutorResponse>(ErrosDeAutor.NaoEncontrado(id));
        }

        var nomeAlterado = autor.AlterarNome(request.Nome);
        if (nomeAlterado.IsFailure)
        {
            return Result.Failure<AutorResponse>(nomeAlterado.Error);
        }

        autorRepository.Atualizar(autor);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success(autor.ParaResponse());
    }

    public async Task<Result> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var autor = await autorRepository.ObterPorIdAsync(id, cancellationToken);
        if (autor is null)
        {
            return Result.Failure(ErrosDeAutor.NaoEncontrado(id));
        }

        // A regra "autor com livros não pode ser excluído" mora no serviço de domínio,
        // não aqui: este método só decide a ordem dos passos.
        var remocaoPermitida = await autorDomainService.GarantirRemocaoPermitidaAsync(id, cancellationToken);
        if (remocaoPermitida.IsFailure)
        {
            return remocaoPermitida;
        }

        autorRepository.Remover(autor);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success();
    }
}
