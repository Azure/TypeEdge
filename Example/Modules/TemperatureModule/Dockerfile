FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY build build
COPY Modules/TemperatureModule/TemperatureModule.csproj Modules/TemperatureModule/
COPY Thermostat.Shared/Thermostat.Shared.csproj Thermostat.Shared/
RUN dotnet restore Modules/TemperatureModule/TemperatureModule.csproj -s /src/build -s https://api.nuget.org/v3/index.json
COPY . .
WORKDIR /src/Modules/TemperatureModule
RUN dotnet build TemperatureModule.csproj -c Release -o /app
 
FROM build AS publish
RUN dotnet publish TemperatureModule.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TemperatureModule.dll"]
