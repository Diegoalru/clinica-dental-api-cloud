FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["clinica-dental-api-cloud/clinica-dental-api-cloud.csproj", "clinica-dental-api-cloud/"]
RUN dotnet restore "clinica-dental-api-cloud/clinica-dental-api-cloud.csproj"
COPY . .
WORKDIR "/src/clinica-dental-api-cloud"
RUN dotnet build "./clinica-dental-api-cloud.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./clinica-dental-api-cloud.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "clinica-dental-api-cloud.dll"]
