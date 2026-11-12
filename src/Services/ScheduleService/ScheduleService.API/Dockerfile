FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ServiceDefaults/ src/ServiceDefaults/
COPY src/Services/ScheduleService/ src/Services/ScheduleService/
RUN dotnet publish src/Services/ScheduleService/ScheduleService.API/ScheduleService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ScheduleService.API.dll"]
