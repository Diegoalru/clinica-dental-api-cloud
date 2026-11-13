# Se compila en la arquitectura del runner: la salida es portable y corre en amd64 y arm64
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["clinica-dental-api-cloud/clinica-dental-api-cloud.csproj", "clinica-dental-api-cloud/"]
RUN dotnet restore "clinica-dental-api-cloud/clinica-dental-api-cloud.csproj"
COPY . .
WORKDIR "/src/clinica-dental-api-cloud"
RUN dotnet publish "./clinica-dental-api-cloud.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Imagen chiseled: sin shell ni gestor de paquetes, usuario sin privilegios
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "clinica-dental-api-cloud.dll"]
