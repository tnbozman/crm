FROM mcr.microsoft.com/devcontainers/dotnet:10.0

# OS tools: make, jq, unzip, sqlcmd
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get install -y --no-install-recommends make jq unzip curl gnupg2 \
    && rm -rf /var/lib/apt/lists/*

# sqlcmd via mssql-tools18 (Debian Bookworm)
RUN curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
        | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && curl -fsSL https://packages.microsoft.com/config/debian/12/prod.list \
        > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends mssql-tools18 unixodbc-dev \
    && ln -sf /opt/mssql-tools18/bin/sqlcmd /usr/local/bin/sqlcmd \
    && rm -rf /var/lib/apt/lists/*

# Warm the NuGet cache so the first restore is offline-fast.
# Copy only the central package files; a warm-up project is added in S0-2 once
# Directory.Packages.props is finalized and all package versions are known.
