using Biblioteca.Application.Contratos;
using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Application.Mapeamentos;
using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Erros;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;

namespace Biblioteca.Application.Servicos;

/// <inheritdoc cref="IGeneroAppService"/>
public sealed class GeneroAppService(
    IGeneroRepository generoRepository,
    IGeneroDomainService generoDomainService,
    IUnitOfWork unitOfWork) : IGeneroAppService
{
    public async Task<Result<GeneroResponse>> CriarAsync(
        CriarGeneroRequest request,
        CancellationToken cancellationToken = default)
    {
        // Primeiro o que não custa ida ao banco: se o nome é inválido,
        // não faz sentido consultar duplicidade.
        var generoCriado = Genero.Criar(request.Nome);
        if (generoCriado.IsFailure)
        {
            return Result.Failure<GeneroResponse>(generoCriado.Error);
        }

        var nomeDisponivel = await generoDomainService.GarantirNomeDisponivelAsync(
            generoCriado.Value.Nome,
            cancellationToken: cancellationToken);

        if (nomeDisponivel.IsFailure)
        {
            return Result.Failure<GeneroResponse>(nomeDisponivel.Error);
        }

        await generoRepository.AdicionarAsync(generoCriado.Value, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success(generoCriado.Value.ParaResponse());
    }

    public async Task<Result<GeneroResponse>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var genero = await generoRepository.ObterPorIdAsync(id, cancellationToken);

        return genero is null
            ? Result.Failure<GeneroResponse>(ErrosDeGenero.NaoEncontrado(id))
            : Result.Success(genero.ParaResponse());
    }

    public async Task<IReadOnlyList<GeneroResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var generos = await generoRepository.ListarAsync(cancellationToken);

        return generos.ParaResponse();
    }

    public async Task<Result<GeneroResponse>> AtualizarAsync(
        Guid id,
        AtualizarGeneroRequest request,
        CancellationToken cancellationToken = default)
    {
        var genero = await generoRepository.ObterPorIdAsync(id, cancellationToken);
        if (genero is null)
        {
            return Result.Failure<GeneroResponse>(ErrosDeGenero.NaoEncontrado(id));
        }

        var nomeAlterado = genero.AlterarNome(request.Nome);
        if (nomeAlterado.IsFailure)
        {
            return Result.Failure<GeneroResponse>(nomeAlterado.Error);
        }

        // O próprio gênero é excluído da checagem: renomear "Ficcao" para "Ficção"
        // não pode colidir consigo mesmo.
        var nomeDisponivel = await generoDomainService.GarantirNomeDisponivelAsync(
            genero.Nome,
            idIgnorado: genero.Id,
            cancellationToken);

        if (nomeDisponivel.IsFailure)
        {
            return Result.Failure<GeneroResponse>(nomeDisponivel.Error);
        }

        generoRepository.Atualizar(genero);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success(genero.ParaResponse());
    }

    public async Task<Result> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var genero = await generoRepository.ObterPorIdAsync(id, cancellationToken);
        if (genero is null)
        {
            return Result.Failure(ErrosDeGenero.NaoEncontrado(id));
        }

        var remocaoPermitida = await generoDomainService.GarantirRemocaoPermitidaAsync(id, cancellationToken);
        if (remocaoPermitida.IsFailure)
        {
            return remocaoPermitida;
        }

        generoRepository.Remover(genero);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success();
    }
}
