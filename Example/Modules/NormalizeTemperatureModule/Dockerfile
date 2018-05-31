FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Modules/NormalizeTemperatureModule/NormalizeTemperatureModule.csproj Modules/NormalizeTemperatureModule/
COPY Thermostat.Shared/Thermostat.Shared.csproj Thermostat.Shared/
COPY NuGet.Config ./
RUN dotnet restore Modules/NormalizeTemperatureModule/NormalizeTemperatureModule.csproj
COPY . . 
WORKDIR /src/Modules/NormalizeTemperatureModule
RUN dotnet build NormalizeTemperatureModule.csproj -c Release -o /app
  
FROM build AS publish
RUN dotnet publish NormalizeTemperatureModule.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NormalizeTemperatureModule.dll"]
 