# syntax=docker/dockerfile:1
# Multi-stage build for CarRental.UserService
# Build context = repository root (needs nuget.config + CarRental.Shared):
#   docker build -f CarRental.UserService/Dockerfile -t carrental-user-service .

# ---------- Stage 1: build (full .NET SDK, never shipped) ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# nuget.config registers ./local-nuget-feed as a source. NuGet fails if a local
# source folder doesn't exist, so create it before the first restore.
COPY nuget.config ./
RUN mkdir -p local-nuget-feed

# 1) Pack CarRental.Shared into the local feed (same as the README build order).
#    Copy the .csproj first so the restore layer is cached until it changes.
COPY CarRental.Shared/CarRental.Shared.csproj CarRental.Shared/
RUN dotnet restore CarRental.Shared/CarRental.Shared.csproj
COPY CarRental.Shared/ CarRental.Shared/
RUN dotnet pack CarRental.Shared/CarRental.Shared.csproj -c Release --no-restore \
        -o local-nuget-feed -p:GeneratePackageOnBuild=false

# 2) Restore the service (pulls CarRental.Shared from local-nuget-feed).
COPY CarRental.UserService/CarRental.UserService.csproj CarRental.UserService/
RUN dotnet restore CarRental.UserService/CarRental.UserService.csproj

# 3) Copy the source and publish.
COPY CarRental.UserService/ CarRental.UserService/
RUN dotnet publish CarRental.UserService/CarRental.UserService.csproj -c Release --no-restore \
        -o /app/publish -p:UseAppHost=false

# ---------- Stage 2: runtime (ASP.NET runtime only, much smaller) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The .NET 8 images listen on 8080 by default and ship a non-root "app" user.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "CarRental.UserService.dll"]
