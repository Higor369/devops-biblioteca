namespace Biblioteca.Api.IntegrationTests.Suporte;

/// <summary>
/// Agrupa todas as classes de teste de integração em uma coleção só.
/// <para>
/// Sem isto, o xUnit criaria uma instância de fixture por classe — ou seja, um
/// container PostgreSQL por classe de teste. A coleção compartilha um único container
/// e, como o xUnit executa os testes de uma mesma coleção em série, a limpeza do banco
/// entre testes continua determinística.
/// </para>
/// </summary>
[CollectionDefinition(Nome)]
public sealed class ColecaoDeIntegracao : ICollectionFixture<BibliotecaApiFactory>
{
    public const string Nome = "Integração";
}
