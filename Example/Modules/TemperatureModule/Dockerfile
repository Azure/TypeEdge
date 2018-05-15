FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Example/Modules/TemperatureModule/TemperatureModule.csproj Example/Modules/TemperatureModule/
COPY Microsoft.Azure.IoT.TypeEdge/Microsoft.Azure.IoT.TypeEdge.csproj Microsoft.Azure.IoT.TypeEdge/
COPY Example/Thermostat.Shared/Thermostat.Shared.csproj Example/Thermostat.Shared/
RUN dotnet restore Example/Modules/TemperatureModule/TemperatureModule.csproj
COPY . .
WORKDIR /src/Example/Modules/TemperatureModule
RUN dotnet build TemperatureModule.csproj -c Release -o /app
 
FROM build AS publish
RUN dotnet publish TemperatureModule.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TemperatureModule.dll"]
