ARG base_tag=2.1.0-runtime-bionic
FROM microsoft/dotnet:${base_tag} AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Modules/Preprocessor/Preprocessor.csproj Modules/Preprocessor/
COPY Thermostat.Shared/Thermostat.Shared.csproj Thermostat.Shared/
COPY NuGet.Config ./
RUN dotnet restore Modules/Preprocessor/Preprocessor.csproj
COPY . . 
WORKDIR /src/Modules/Preprocessor
RUN dotnet build Preprocessor.csproj -c Release -o /app
  
FROM build AS publish
RUN dotnet publish Preprocessor.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Preprocessor.dll"]
 