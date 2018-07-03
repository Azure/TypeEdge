ARG base_tag=2.1-runtime-stretch-slim
FROM microsoft/dotnet:${base_tag} AS base

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Modules/TypeEdgeModule1/TypeEdgeModule1.csproj Modules/TypeEdgeModule1/
COPY TypeEdgeApplication.Shared/TypeEdgeApplication.Shared.csproj TypeEdgeApplication.Shared/
COPY NuGet.Config ./
RUN dotnet restore Modules/TypeEdgeModule1/TypeEdgeModule1.csproj 
COPY . .
WORKDIR /src/Modules/TypeEdgeModule1
RUN dotnet build TypeEdgeModule1.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish TypeEdgeModule1.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TypeEdgeModule1.dll"]
