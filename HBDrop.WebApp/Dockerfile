# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080

# Set environment variables (HTTP only for Docker)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "HBDrop.WebApp.dll"]
