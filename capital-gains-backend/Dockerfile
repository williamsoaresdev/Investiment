# Use a imagem base do .NET 8 Runtime (Alpine para menor tamanho)
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS runtime

# Use a imagem SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Defina o diretório de trabalho
WORKDIR /app

# Copie os arquivos de projeto primeiro (para cache de layers)
COPY ["src/CapitalGains.Console/CapitalGains.Console.csproj", "src/CapitalGains.Console/"]
COPY ["src/CapitalGains.Application/CapitalGains.Application.csproj", "src/CapitalGains.Application/"]
COPY ["src/CapitalGains.Domain/CapitalGains.Domain.csproj", "src/CapitalGains.Domain/"]
COPY ["src/CapitalGains.Infrastructure/CapitalGains.Infrastructure.csproj", "src/CapitalGains.Infrastructure/"]

# Restaure as dependências
RUN dotnet restore "src/CapitalGains.Console/CapitalGains.Console.csproj"

# Copie todo o código fonte
COPY . .

# Build da aplicação
WORKDIR /app/src/CapitalGains.Console
RUN dotnet build "CapitalGains.Console.csproj" -c Release -o /app/build

# Publique a aplicação
RUN dotnet publish "CapitalGains.Console.csproj" -c Release -o /app/publish --no-restore

# Imagem final de runtime
FROM runtime AS final
WORKDIR /app

# Copie os binários publicados
COPY --from=build /app/publish .

# Configure o ponto de entrada
ENTRYPOINT ["dotnet", "capital-gains.dll"]