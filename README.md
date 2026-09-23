# Biblioteca — API de gerenciamento de acervo

API REST em .NET 10 para cadastro de **livros**, **autores** e **gêneros**, construída como
base para um projeto de estudos de DevOps. A prioridade aqui é a estrutura: arquitetura em
camadas com regra de dependência aplicada pelo compilador, modelagem seguindo DDD,
princípios SOLID e cobertura de testes que roda contra infraestrutura real.

## Stack

| Item | Escolha |
| --- | --- |
| Runtime | .NET 10 (`net10.0`) |
| API | ASP.NET Core com controllers |
| Persistência | EF Core 10 + PostgreSQL 17 (Npgsql) |
| Mapeamento | Mapperly (source generator, MIT) |
| Validação de entrada | DataAnnotations |
| Documentação | OpenAPI + Scalar |
| Testes | xUnit, NSubstitute, Shouldly, Testcontainers |
| Empacotamento | Docker multi-stage + Docker Compose |

## Como executar

### Ambientes: desenvolvimento e produção

A imagem da API é **uma só** para os dois ambientes — o que muda é a configuração
entregue a ela quando o container sobe. Essa configuração fica dividida em arquivos que o
Compose mescla, o de cima sobrescrevendo o de baixo:

| Arquivo | Papel |
| --- | --- |
| `docker-compose.yml` | Base: o que é igual em qualquer ambiente. Não sobe sozinho. |
| `docker-compose.override.yml` | Desenvolvimento. Carregado **automaticamente** por `docker compose`, por convenção do nome. |
| `docker-compose.prod.yml` | Produção. Só entra quando pedido com `-f`. |
| `.env.prod.example` | Modelo das senhas de produção. O `.env.prod` real nunca é versionado. |

O que cada ambiente recebe:

| | Desenvolvimento | Produção |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| Scalar (`/scalar/v1`) | disponível | `404` |
| Log do SQL gerado pelo EF | ligado (`appsettings.Development.json`) | desligado |
| Acervo de exemplo na criação do banco | gravado (`appsettings.Development.json`) | não |
| Senha do banco | fixa no override | lida do `.env.prod`; se faltar, o Compose recusa subir |
| Porta 5432 do PostgreSQL | aberta para a máquina | fechada; só a API alcança o banco |
| Porta da API | `8080` | `8081` |
| Reinício automático | não | `restart: unless-stopped` |
| Projeto (prefixo de containers, rede e volume) | `biblioteca-dev` | `biblioteca-prod` |

Nomes de projeto diferentes fazem cada ambiente ter o próprio volume de dados. Sem isso, o
de produção encontraria o banco já inicializado com a senha de desenvolvimento — o
PostgreSQL só lê `POSTGRES_PASSWORD` na primeira vez que cria o volume.

#### Desenvolvimento

```bash
docker compose up -d --build
```

A API sobe em `http://localhost:8080` e aplica as migrations sozinha na inicialização —
criando o banco, se ele ainda não existir. O Compose só inicia a API depois que o
healthcheck do PostgreSQL passa.

Num banco recém-criado, a API grava um acervo de exemplo (10 livros, 7 autores e 4 gêneros),
definido em `src/Biblioteca.Infrastructure/Persistencia/DadosDeExemplo.cs`. Se o banco já
tiver qualquer autor ou gênero, nada é gravado. Um volume criado antes dessa mudança já tem
o schema aplicado e não recebe o acervo; para recomeçar do zero, use `docker compose down -v`.

- Documentação interativa: <http://localhost:8080/scalar/v1>
- Healthcheck: <http://localhost:8080/health>

Para derrubar tudo, inclusive os dados:

```bash
docker compose down -v
```

#### Produção (simulada localmente)

```bash
cp .env.prod.example .env.prod   # uma vez só; depois troque a senha
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d --build
```

A API sobe em `http://localhost:8081`. O healthcheck responde normalmente; o Scalar, não.
Os mesmos `-f` e `--env-file` valem para qualquer outro comando nesse ambiente, como
`logs`, `ps` e `down`.

#### Vendo o que cada ambiente recebe

Trocar `up -d --build` por `config` imprime o resultado final da mesclagem, sem subir nada:

```bash
docker compose config
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod config
```

### Rodando a API local, com o banco em container

```bash
docker compose up -d banco
dotnet run --project src/Biblioteca.Api
```

A connection string de desenvolvimento já aponta para `localhost:5432`
(veja `src/Biblioteca.Api/appsettings.Development.json`).

### Postman

A pasta `postman/` tem uma coleção com todos os endpoints e dois ambientes:

| Arquivo | Conteúdo |
| --- | --- |
| `Biblioteca.postman_collection.json` | Os 26 pedidos: CRUD de autores, gêneros e livros, healthcheck e cenários de erro |
| `Biblioteca-Dev.postman_environment.json` | `baseUrl` = `http://localhost:8080` |
| `Biblioteca-Prod.postman_environment.json` | `baseUrl` = `http://localhost:8081` |

Importe os três no Postman (*Import*) e selecione o ambiente. Os pedidos *Criar* guardam o id
devolvido em `autorId`, `generoId` e `livroId`, usados pelos demais. As pastas estão na ordem
certa para o *Run collection*: cria um autor, um gênero e um livro, testa os erros e apaga o que
criou no fim — dá para repetir quantas vezes quiser.

### Testes

```bash
dotnet test
```

Os testes de integração sobem um PostgreSQL próprio via Testcontainers,
então **é necessário ter o Docker em execução**.

### Migrations

A ferramenta está fixada como tool local do repositório (`dotnet-tools.json`):

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/Biblioteca.Infrastructure --output-dir Persistencia/Migrations
```

## Arquitetura

Oito projetos: cinco de produção e três de teste.

```
Biblioteca.Api  ──►  Biblioteca.Application  ──►  Biblioteca.Domain.Services  ──►  Biblioteca.Domain
      │                                                                                   ▲
      └──────────────►  Biblioteca.Infrastructure  ─────────────────────────────────────►─┘
                            (implementa os contratos declarados no domínio)
```

| Projeto | Responsabilidade | Depende de |
| --- | --- | --- |
| `Biblioteca.Domain` | Entidades, objetos de valor, catálogo de erros, contratos de repositório | **nada** |
| `Biblioteca.Domain.Services` | Regras que cruzam agregados (exclusão bloqueada, unicidade de gênero, existência de autor/gênero) | Domain |
| `Biblioteca.Application` | Casos de uso, DTOs, mapeamentos | Domain, Domain.Services |
| `Biblioteca.Infrastructure` | `DbContext`, mapeamentos EF Core, repositórios, migrations, auditoria | Domain |
| `Biblioteca.Api` | Controllers, tradução de erro para HTTP, composition root | Application, Infrastructure |

**A regra de dependência é verificada pelo compilador.** O `Biblioteca.Domain` não tem um
único `PackageReference` — tentar usar `DbContext` ou `IActionResult` dentro dele não compila.
A única seta que aponta "para fora" é `Api → Infrastructure`, e existe apenas no `Program.cs`,
que é onde as interfaces são amarradas às implementações concretas.

## Modelagem: entidades e objetos de valor

O domínio tem duas raízes, e a diferença entre elas é o ponto central do DDD.

### `Entity` — o que tem identidade

```csharp
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; }
    public DateTimeOffset CriadoEm { get; }
    public DateTimeOffset? AtualizadoEm { get; }
}
```

Uma entidade **é** o seu id, não o seu conteúdo: dois livros com o mesmo título, autor e ano
continuam sendo livros diferentes. Por isso a igualdade é comparada pelo `Id`, e entidades
de tipos diferentes nunca são iguais mesmo compartilhando o id.

`Autor`, `Genero` e `Livro` herdam daqui.

**As datas de auditoria são preenchidas pela infraestrutura**, num
`SaveChangesInterceptor`, e não pelo domínio. Data de gravação é rastro de persistência, não
regra de negócio — e delegar isso ao interceptador torna impossível esquecer: qualquer
entidade nova, ou qualquer método de alteração criado no futuro, já nasce com as datas
corretas. A escrita passa pelo rastreador do EF Core, então os `set` privados continuam
fechados para o resto do código.

### `ValueObject` — o que é o próprio conteúdo

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> ObterComponentesDeIgualdade();
}
```

Um objeto de valor não tem identidade: dois nomes de autor com o mesmo texto são o mesmo
nome. A classe derivada só informa quais valores a compõem — igualdade, hash e operadores
saem daí. São imutáveis por construção: não há método de alteração, quem quer outro valor
cria outra instância.

| Objeto de valor | Usado em | Valida |
| --- | --- | --- |
| `NomeDeAutor` | `Autor.Nome` | obrigatório, 2–200 caracteres, sem espaços nas bordas |
| `NomeDeGenero` | `Genero.Nome` | obrigatório, 2–100 caracteres, sem espaços nas bordas |
| `TituloDeLivro` | `Livro.Titulo` | obrigatório, 2–300 caracteres, sem espaços nas bordas |
| `AnoDePublicacao` | `Livro.AnoDePublicacao` | entre 1450 e o ano corrente |

Tipar em vez de usar `string` solta traz duas garantias: a validação acontece uma vez só, no
ponto de criação, e fica impossível passar um título de livro onde se espera um nome de autor
— o compilador recusa.

No banco cada um vira uma coluna comum, via `HasConversion`. Na volta são reconstituídos sem
revalidar: o dado passou pela validação quando entrou, e revalidar na leitura só criaria o
risco de um registro antigo ficar irrecuperável ao apertarmos uma regra.

### Por que gênero **não** é um objeto de valor

Gênero tem ciclo de vida próprio: é criado, renomeado e excluído por conta própria, tem
endpoints e é referenciado por id. Isso é definição de entidade. Transformá-lo em objeto de
valor dentro de `Livro` eliminaria o CRUD de gêneros, a unicidade de nome e o 409 ao tentar
excluir um gênero em uso. Quem é objeto de valor aqui é o `NomeDeGenero` que ele carrega.

## As duas camadas de validação

| Camada | O que barra | Resposta |
| --- | --- | --- |
| `DataAnnotations` nos DTOs | Requisição malformada: campo ausente, texto longo demais, `Guid` vazio, ano abaixo de 1450 | `400` com a lista de campos em `errors` e `codigo: "requisicao.invalida"` |
| Objetos de valor e entidades | Invariante de negócio, já sobre o dado normalizado | `400` com o `codigo` específico, ex.: `autor.nome_tamanho_invalido` |
| Serviços de domínio | Regra que depende do banco: referência inexistente, nome duplicado, exclusão bloqueada | `404` ou `409` com o `codigo` específico |

**Os limites das anotações vêm das constantes dos objetos de valor**, então as duas camadas
não têm como divergir:

```csharp
public sealed record CriarAutorRequest(
    [Required(ErrorMessage = "O nome do autor é obrigatório.")]
    [StringLength(NomeDeAutor.TamanhoMaximo, MinimumLength = NomeDeAutor.TamanhoMinimo)]
    string Nome);
```

Ainda assim há casos que só o domínio pega. `" a "` tem três caracteres e passa no
`StringLength`, mas vira `"a"` depois da normalização — e aí é o objeto de valor que recusa.
Há um teste de integração para exatamente esse caso.

> **Atenção com record posicional:** o atributo precisa ficar no **parâmetro**, não em
> `[property: ...]`. Com `[property:]` o ASP.NET lança em tempo de execução dizendo que a
> anotação seria ignorada, e todo endpoint responde 500.

Dois detalhes que valem registro: `[Required]` descarta os espaços das bordas antes de
decidir, então `"   "` já é barrado ali; e `[Required]` não serve para `Guid`, porque a
ausência do campo chega como `Guid.Empty` — daí a anotação própria `[GuidNaoVazio]`.

## Mapeamento entidade → DTO

Feito com **Mapperly**, escolhido em vez do AutoMapper por dois motivos: é MIT de verdade
(o AutoMapper passou a exigir licença comercial acima de um limite de faturamento em 2025) e
é um *source generator* — o código é gerado na compilação, sem reflection em execução, e um
campo que deixe de existir vira erro de compilação em vez de exceção em produção.

O gerador é estrito por padrão: campo de origem sem destino quebra o build, e ignorar precisa
ser declarado (`[MapperIgnoreSource]`). Foi isso que apontou, ainda na compilação, que
`AutorId` e `GeneroId` não iam para a resposta — no caso, de propósito, porque os objetos
completos já vão.

**O caminho inverso, requisição → entidade, não passa pelo mapeador de propósito.** Entidade
só nasce pelo método de fábrica que valida e devolve `Result`. Um mapeador que preenchesse as
propriedades diretamente furaria exatamente a garantia que o domínio existe para dar.

## Endpoints

Os três recursos expõem o mesmo CRUD, em `/api/autores`, `/api/generos` e `/api/livros`.

| Verbo | Rota | Respostas |
| --- | --- | --- |
| `GET` | `/api/{recurso}` | `200` |
| `GET` | `/api/{recurso}/{id}` | `200`, `404` |
| `POST` | `/api/{recurso}` | `201`, `400`, `404`¹, `409`² |
| `PUT` | `/api/{recurso}/{id}` | `200`, `400`, `404`, `409`² |
| `DELETE` | `/api/{recurso}/{id}` | `204`, `404`, `409`³ |

¹ Em livros, quando o autor ou o gênero informado não existe.
² Em gêneros, quando o nome já está cadastrado.
³ Em autores e gêneros, quando ainda há livros vinculados.

Erros seguem RFC 9457, com `Content-Type: application/problem+json`, e carregam um campo
extra `codigo` — estável e legível por máquina, para que um cliente reaja a
`autor.possui_livros_vinculados` sem depender do texto da mensagem:

```json
{
  "title": "Conflito com o estado atual",
  "status": 409,
  "detail": "Não é possível excluir o autor: existem 1 livro(s) vinculado(s) a ele.",
  "instance": "/api/autores/01a0cb39-22e5-7fae-858e-8af22cc8139e",
  "codigo": "autor.possui_livros_vinculados"
}
```

Toda resposta de sucesso traz as datas de auditoria:

```json
{
  "id": "01a0cb39-22e5-7fae-858e-8af22cc8139e",
  "nome": "Machado de Assis",
  "criadoEm": "2026-09-22T22:25:14.552211+00:00",
  "atualizadoEm": null
}
```

## Regras de negócio

- Todo livro exige `autorId` e `generoId`, e ambos precisam existir no acervo.
  Referência inexistente responde `404`, não `400`: o que falta é o recurso apontado,
  não a requisição que está malformada.
- **Livros repetidos são permitidos.** Não existe índice único sobre título/autor/gênero;
  dois exemplares da mesma obra são registros distintos, separados apenas pelo `Id`.
- Autor e gênero com livros vinculados **não podem ser excluídos** (`409`). A mensagem
  informa quantos livros bloqueiam a operação.
- Nome de gênero é único, ignorando maiúsculas e minúsculas. Nome de autor **não** é único:
  homônimos existem e, sem data de nascimento ou nacionalidade, não haveria como distingui-los.
- Ano de publicação vai de 1450 (prensa de Gutenberg) ao ano corrente.

## Decisões e seus custos

**`Result` em vez de exceções para falhas de negócio.** Título inválido e autor inexistente
são desfechos esperados do fluxo, não defeitos, e não pagam o custo de uma exceção. Exceção
ficou reservada ao que é realmente excepcional, capturado por um handler global que devolve
`500` sem vazar detalhe interno. *Custo:* cada caso de uso carrega um `if (resultado.IsFailure)`
explícito, o que é mais verboso que deixar uma exceção subir sozinha.

**Auditoria por interceptador, não pelo domínio.** Mantém o domínio sem dependência de
relógio e torna impossível esquecer de carimbar. *Custo:* uma entidade recém-criada em memória
tem `CriadoEm` zerado até ser gravada, então essa garantia só aparece em teste de integração.

**Carimbos truncados ao microssegundo.** `DateTimeOffset` guarda até 100 nanossegundos, mas
`timestamptz` do PostgreSQL só grava microssegundos. Sem truncar, o valor devolvido logo após
o cadastro seria ligeiramente diferente do mesmo campo lido depois — a API responderia dois
valores distintos para o mesmo registro.

**Livro referencia autor e gênero só por id, sem propriedade de navegação.** Mantém a
fronteira entre agregados nítida e — o que pesou mais — torna a camada de aplicação
testável com dublês, já que navegações só o EF Core consegue preencher. A listagem compõe
a resposta com três consultas fixas (livros, autores, gêneros), sem N+1, e há um teste
que falha se alguém reintroduzir a busca dentro do laço. *Custo:* uma listagem com `JOIN`
seria uma consulta em vez de três.

**Unicidade de gênero em dois lugares.** O serviço de domínio checa antes para devolver um
`409` legível; o índice único sobre uma coluna `citext` no PostgreSQL cobre a corrida entre
duas requisições simultâneas, que nenhuma checagem em memória consegue cobrir. Redundância
deliberada: uma dá boa mensagem, a outra dá garantia.

**Sem FluentValidation.** As anotações cobrem o formato da requisição e as invariantes vivem
nos objetos de valor. Uma terceira camada de validação criaria mais uma fonte de verdade para
divergir.

**`Guid` v7 gerado no domínio.** Sequencial no tempo, então não fragmenta o índice do
PostgreSQL como um v4 faria, e o id existe antes de tocar o banco — o que mantém a entidade
testável sem infraestrutura.

**`TimeProvider` injetado.** A validação de "ano no futuro" depende do ano corrente. Com
`DateTime.Now` dentro do domínio, o teste dessa regra passaria a depender de quando roda.

**Migrations aplicadas na subida da API.** Conveniência de projeto de estudo, controlada por
`Banco:AplicarMigrationsNoStartup`. Em produção isso normalmente vira um passo próprio do
pipeline, para que uma migration demorada ou malsucedida não derrube a aplicação.

**Acervo de exemplo pelo `UseAsyncSeeding` do EF Core, não por `HasData`.** Roda dentro do
`MigrateAsync`, na mesma transação e sob o mesmo lock das migrations, e os registros nascem
pelos métodos de fábrica do domínio — um dado de exemplo inválido falha na hora, em vez de
entrar no banco por uma porta que a API não oferece. `HasData` congelaria o acervo dentro de
uma migration e o levaria também para produção. Controlado por
`Banco:PopularComDadosDeExemplo`, ligado só em desenvolvimento. *Custo:* o seeding roda antes
de o Npgsql recarregar o catálogo de tipos, então precisa recarregá-lo por conta própria para
conseguir gravar na coluna `citext` de gênero num banco recém-criado.

## Testes

```
tests/Biblioteca.Domain.Tests          63 testes   classes base, objetos de valor, entidades e serviços de domínio
tests/Biblioteca.Application.Tests     17 testes   casos de uso, com repositórios simulados
tests/Biblioteca.Api.IntegrationTests  36 testes   API real + PostgreSQL real em container, criação do banco e acervo de exemplo
```

Os testes de integração não substituem nada por dublê: exercitam controller, validação de
entrada, caso de uso, serviço de domínio, EF Core e as constraints do banco. Um provider
in-memory não cobriria isso — ele não valida SQL, nem o tipo `citext`, nem chave estrangeira.
Todas as classes de integração compartilham um único container, e o banco é truncado antes de
cada teste para que a ordem de execução não influencie o resultado.

`TreatWarningsAsErrors` está ligado em toda a solution: um aviso quebra o build.
