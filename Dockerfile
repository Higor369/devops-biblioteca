# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Estagio de build: o SDK completo fica aqui e nao vai para a imagem final.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copia primeiro apenas os arquivos de projeto. Enquanto eles nao mudarem, o Docker
# reaproveita a camada de restore e nao baixa os pacotes de novo a cada build.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Biblioteca.Domain/Biblioteca.Domain.csproj src/Biblioteca.Domain/
COPY src/Biblioteca.Domain.Services/Biblioteca.Domain.Services.csproj src/Biblioteca.Domain.Services/
COPY src/Biblioteca.Application/Biblioteca.Application.csproj src/Biblioteca.Application/
COPY src/Biblioteca.Infrastructure/Biblioteca.Infrastructure.csproj src/Biblioteca.Infrastructure/
COPY src/Biblioteca.Api/Biblioteca.Api.csproj src/Biblioteca.Api/
RUN dotnet restore src/Biblioteca.Api/Biblioteca.Api.csproj

COPY src/ src/
RUN dotnet publish src/Biblioteca.Api/Biblioteca.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# ---------------------------------------------------------------------------
# Imagem final: apenas o runtime do ASP.NET, sem SDK nem codigo-fonte.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

COPY --from=build /app .

# Roda como usuario sem privilegios (ja definido nas imagens oficiais da Microsoft).
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "Biblioteca.Api.dll"]
