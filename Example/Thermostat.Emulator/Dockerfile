FROM microsoft/dotnet:2.1-runtime AS base

# Add an unprivileged user account for running Edge Hub
RUN useradd -ms /bin/bash edgehubuser
ENV EdgeHubUser=edgehubuser

ARG EXE_DIR=.
ENV SSL_CERTIFICATE_PATH=/app/certs
ENV SSL_CERTIFICATE_NAME=mqtt-server.pfx

# Install snappy and set up symlinks that are absent from the base image
# Required by RocksDb
RUN apt-get update && \
    apt-get install -y libsnappy1v5 libcap2-bin && \
    ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so && \
    ln -s /lib/x86_64-linux-gnu/libc.so.6 /usr/lib/x86_64-linux-gnu/libc.so && \
    rm -rf /var/lib/apt/lists/*

# add the CAP_NET_BIND_SERVICE capability to the dotnet binary because
# we are starting edge hub as a non-root user
RUN setcap 'cap_net_bind_service=+ep' /usr/share/dotnet/dotnet

WORKDIR /app

COPY $EXE_DIR/ ./

# Expose MQTT and HTTPS ports
EXPOSE 8883/tcp
EXPOSE 443/tcp

 
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Example/ThermostatApplication/ThermostatApplication.csproj Example/ThermostatApplication/
COPY Example/ThermostatApplication.Shared/ThermostatApplication.Shared.csproj Example/ThermostatApplication.Shared/
COPY Microsoft.Azure.IoT.TypeEdge/Microsoft.Azure.IoT.TypeEdge.csproj Microsoft.Azure.IoT.TypeEdge/
COPY Microsoft.Azure.IoT.TypeEdge.Host/Microsoft.Azure.IoT.TypeEdge.Host.csproj Microsoft.Azure.IoT.TypeEdge.Host/
COPY Example/TemperatureModule/TemperatureModule.csproj Example/TemperatureModule/
COPY Example/NormalizeTemperatureModule/NormalizeTemperatureModule.csproj Example/NormalizeTemperatureModule/
RUN dotnet restore Example/ThermostatApplication/ThermostatApplication.csproj
COPY . .
WORKDIR /src/Example/ThermostatApplication
RUN dotnet build ThermostatApplication.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ThermostatApplication.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "ThermostatApplication.dll"]
