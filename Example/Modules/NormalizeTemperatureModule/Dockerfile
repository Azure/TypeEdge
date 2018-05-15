FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Example/Modules/NormalizeTemperatureModule/NormalizeTemperatureModule.csproj Example/Modules/NormalizeTemperatureModule/
COPY Microsoft.Azure.IoT.TypeEdge/Microsoft.Azure.IoT.TypeEdge.csproj Microsoft.Azure.IoT.TypeEdge/
COPY Example/Thermostat.Shared/Thermostat.Shared.csproj Example/Thermostat.Shared/
RUN dotnet restore Example/Modules/NormalizeTemperatureModule/NormalizeTemperatureModule.csproj
COPY . .
WORKDIR /src/Example/Modules/NormalizeTemperatureModule
RUN dotnet build NormalizeTemperatureModule.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish NormalizeTemperatureModule.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NormalizeTemperatureModule.dll"]
