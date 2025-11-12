# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY HBDrop.WebApp/HBDrop.WebApp.csproj HBDrop.WebApp/
RUN dotnet restore "HBDrop.WebApp/HBDrop.WebApp.csproj"

# Copy everything else and build
COPY HBDrop.WebApp/ HBDrop.WebApp/
WORKDIR /src/HBDrop.WebApp
RUN dotnet build "HBDrop.WebApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HBDrop.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080

# Set environment variables (HTTP only for Docker)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "HBDrop.WebApp.dll"]
