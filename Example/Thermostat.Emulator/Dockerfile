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


FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Example/Thermostat.Emulator/Thermostat.Emulator.csproj Example/Thermostat.Emulator/
COPY Example/Thermostat.Shared/Thermostat.Shared.csproj Example/Thermostat.Shared/
COPY Microsoft.Azure.IoT.TypeEdge/Microsoft.Azure.IoT.TypeEdge.csproj Microsoft.Azure.IoT.TypeEdge/
COPY Microsoft.Azure.IoT.TypeEdge.Host/Microsoft.Azure.IoT.TypeEdge.Host.csproj Microsoft.Azure.IoT.TypeEdge.Host/
COPY Example/Modules/NormalizeTemperatureModule/NormalizeTemperatureModule.csproj Example/Modules/NormalizeTemperatureModule/
COPY Example/Modules/TemperatureModule/TemperatureModule.csproj Example/Modules/TemperatureModule/
RUN dotnet restore Example/Thermostat.Emulator/Thermostat.Emulator.csproj
COPY . .
WORKDIR /src/Example/Thermostat.Emulator
RUN dotnet build Thermostat.Emulator.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Thermostat.Emulator.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Thermostat.Emulator.dll"]
