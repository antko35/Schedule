# Generic build for any of the three services.
# Build context MUST be the repository root, because every *.API project
# references ..\..\..\Schedule.ServiceDefaults (added by the Aspire migration).
#
# Driven by two build args set per-service in docker-compose.yml:
#   PROJECT - path to the .csproj to publish
#   DLL     - entrypoint assembly name (e.g. UserService.API.dll)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src
COPY . .
RUN dotnet restore "$PROJECT"
RUN dotnet publish "$PROJECT" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ARG DLL
ENV APP_DLL=${DLL}
COPY --from=build /app/publish .
# Shell form so ${APP_DLL} is expanded at runtime.
ENTRYPOINT dotnet ${APP_DLL}
