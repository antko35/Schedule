FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ServiceDefaults/ src/ServiceDefaults/
COPY src/Services/UserManagementService/ src/Services/UserManagementService/
RUN dotnet publish src/Services/UserManagementService/UserManagementService.API/UserManagementService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserManagementService.API.dll"]
