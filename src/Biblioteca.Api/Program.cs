using Biblioteca.Api.Infraestrutura;
using Biblioteca.Application;
using Biblioteca.Domain.Services;
using Biblioteca.Infrastructure;
using Biblioteca.Infrastructure.Persistencia;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Composition root: o único lugar da solução que conhece todas as camadas ao
// mesmo tempo e amarra interface a implementação concreta.
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Biblioteca")
    ?? throw new InvalidOperationException(
        "A connection string 'Biblioteca' não foi configurada. " +
        "Defina ConnectionStrings__Biblioteca no ambiente ou em appsettings.");

var popularComDadosDeExemplo =
    builder.Configuration.GetValue("Banco:PopularComDadosDeExemplo", defaultValue: false);

builder.Services
    .AddInfrastructure(connectionString, popularComDadosDeExemplo)
    .AddDomainServices()
    .AddApplication();

builder.Services.AddControllers();

// Faz o 400 automático das DataAnnotations sair no mesmo formato dos erros de negócio.
builder.Services.AddRespostaDeValidacaoPadronizada();

// ProblemDetails (RFC 9457) como formato padrão de erro em toda a API.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorGlobalDeExcecoes>();

builder.Services.AddOpenApi();

// Usado pelo orquestrador de containers para saber se a aplicação está de pé
// e se o banco responde.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BibliotecaDbContext>("banco");

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Só a interface do Swagger: o documento continua sendo o que o AddOpenApi gera
    // em /openapi/v1.json. RoutePrefix vazio serve a interface na raiz da porta.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Biblioteca API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

if (builder.Configuration.GetValue("Banco:AplicarMigrationsNoStartup", defaultValue: true))
{
    await app.AplicarMigrationsPendentesAsync();
}

await app.RunAsync();

/// <summary>
/// Tornar <c>Program</c> público permite que o projeto de testes de integração use
/// <c>WebApplicationFactory&lt;Program&gt;</c> para subir a API real em memória.
/// </summary>
public partial class Program;
