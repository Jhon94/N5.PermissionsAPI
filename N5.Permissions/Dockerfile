#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/N5.Permissions/N5.Permissions.API.csproj", "src/N5.Permissions.API/"]
COPY ["src/N5.Permissions.Application/N5.Permissions.Application.csproj", "src/N5.Permissions.Application/"]
COPY ["src/N5.Permissions.Domain/N5.Permissions.Domain.csproj", "src/N5.Permissions.Domain/"]
COPY ["src/N5.Permissions.Infrastructure/N5.Permissions.Infrastructure.csproj", "src/N5.Permissions.Infrastructure/"]

# Copy Directory.Packages.props if using central package management
COPY ["Directory.Packages.props", "."]

# Restore dependencies
RUN dotnet restore "src/N5.Permissions.API/N5.Permissions.API.csproj"

# Copy source code
COPY . .

# Build project
WORKDIR "/src/src/N5.Permissions.API"
RUN dotnet build "N5.Permissions.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "N5.Permissions.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081

ENTRYPOINT ["dotnet", "N5.Permissions.API.dll"]