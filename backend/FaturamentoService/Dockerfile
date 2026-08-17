# Esta fase é usada na execução base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
# Configura o Kestrel para rodar na porta 5169 dentro do contêiner
ENV ASPNETCORE_HTTP_PORTS=5169
EXPOSE 5169

# Esta fase compila o projeto
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FaturamentoService.csproj", "."]
RUN dotnet restore "./FaturamentoService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./FaturamentoService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase publica os binários
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FaturamentoService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagem final de execução
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FaturamentoService.dll"]