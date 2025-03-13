# Stage: base � imagem de runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Stage: build � utiliza a imagem do SDK para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia o NuGet.config (que referencia o /src/LocalPackages)
COPY NuGet.config .

# Copia a pasta LocalPackages para que o restore encontre os pacotes locais
COPY LocalPackages ./LocalPackages

# Copia o arquivo de projeto da API
COPY ["TCFiapConsumerUpdateContact.API.csproj", "./"]
RUN dotnet restore "TCFiapConsumerUpdateContact.API.csproj"

# Copia todo o restante do c�digo para o container
COPY . .

# Define o diret�rio de trabalho para o projeto da API e compila
WORKDIR /src
RUN dotnet build "TCFiapConsumerUpdateContact.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage: publish � publica a aplica��o para produ��o
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TCFiapConsumerUpdateContact.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage final � constr�i a imagem de runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TCFiapConsumerUpdateContact.API.dll"]
