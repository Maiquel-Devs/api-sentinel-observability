# Estágio 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copia os arquivos de projeto (.csproj) para restaurar as dependências (otimização de cache de camadas)
COPY src/PaymentFlow.Diagnostics/PaymentFlow.Diagnostics.csproj src/PaymentFlow.Diagnostics/
COPY src/PaymentFlow.Api/PaymentFlow.Api.csproj src/PaymentFlow.Api/

RUN dotnet restore src/PaymentFlow.Api/PaymentFlow.Api.csproj

# Copia todo o código-fonte restante
COPY src/ src/

# Compila e publica a aplicação em modo Release
WORKDIR /app/src/PaymentFlow.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime leve (ASP.NET Core 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Define a porta padrão do ASP.NET Core 10
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PaymentFlow.Api.dll"]