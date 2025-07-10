FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Add curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["Anekdotify.Api/Anekdotify.Api.csproj", "Anekdotify.Api/"]
COPY ["Anekdotify.BL/Anekdotify.BL.csproj", "Anekdotify.BL/"]
COPY ["Anekdotify.Common/Anekdotify.Common.csproj", "Anekdotify.Common/"]
COPY ["Anekdotify.Database/Anekdotify.Database.csproj", "Anekdotify.Database/"]
COPY ["Anekdotify.Models/Anekdotify.Models.csproj", "Anekdotify.Models/"]

# Restore dependencies
RUN dotnet restore "./Anekdotify.Api/Anekdotify.Api.csproj"

# Copy source code
COPY . .
WORKDIR "/src/Anekdotify.Api"

# Build application
RUN dotnet build "./Anekdotify.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Anekdotify.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install .NET SDK and EF Tools in production container
RUN apt-get update && \
    apt-get install -y wget && \
    wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0 && \
    dotnet tool install --global dotnet-ef && \
    rm -rf /var/lib/apt/lists/*

# Add dotnet tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Create non-root user and setup directories
RUN adduser --disabled-password --gecos '' appuser && \
    mkdir -p /app/keys && \
    chown -R appuser:appuser /app

USER appuser

ENTRYPOINT ["dotnet", "Anekdotify.Api.dll"]