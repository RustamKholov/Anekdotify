FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

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

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Anekdotify.Api.dll"]