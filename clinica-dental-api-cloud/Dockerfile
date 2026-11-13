# Imagen del API. Se construye desde la raíz del repositorio (la carpeta de la solución):
#   docker build -f clinica-dental-api-cloud/Dockerfile -t clinica-dental-api .

# --- Etapa 1: compilación ---
# Se compila en la arquitectura del runner: la salida es portable y corre en amd64 y arm64
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Primero solo el .csproj: mientras no cambien los paquetes, Docker reutiliza la capa del restore
COPY ["clinica-dental-api-cloud/clinica-dental-api-cloud.csproj", "clinica-dental-api-cloud/"]
# El restore usa la misma configuración que el publish (el .csproj tiene paquetes que solo existen en Debug)
RUN dotnet restore "clinica-dental-api-cloud/clinica-dental-api-cloud.csproj" -p:Configuration=$BUILD_CONFIGURATION

# Luego el código, solo del proyecto del API (las pruebas no entran en la imagen)
COPY ["clinica-dental-api-cloud/", "clinica-dental-api-cloud/"]
WORKDIR "/src/clinica-dental-api-cloud"
RUN dotnet publish "./clinica-dental-api-cloud.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# --- Etapa 2: ejecución ---
# Imagen chiseled: sin shell ni gestor de paquetes, usuario sin privilegios. No trae tzdata:
# la hora de Costa Rica se resuelve en Services/ClinicTime.cs
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
LABEL org.opencontainers.image.title="clinica-dental-api" \
      org.opencontainers.image.description="API REST del sistema de citas para consultorios dentales" \
      org.opencontainers.image.source="https://github.com/Diegoalru/clinica-dental-api-cloud"

# Logs en JSON por la salida estándar (Twelve-Factor) y sin el canal de diagnóstico, que no se usa
ENV Logging__Console__FormatterName=json \
    DOTNET_EnableDiagnostics=0

USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "clinica-dental-api-cloud.dll"]
