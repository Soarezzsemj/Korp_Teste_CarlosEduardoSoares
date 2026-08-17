FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
# Configura o Kestrel para rodar na porta 5190 dentro do contêiner
ENV ASPNETCORE_HTTP_PORTS=5190
EXPOSE 5190

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EstoqueService.csproj", "."]
RUN dotnet restore "./EstoqueService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./EstoqueService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EstoqueService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EstoqueService.dll"]