# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Cập nhật đường dẫn .csproj chính xác
COPY TAPRIM/TAPrim.csproj TAPRIM/
RUN dotnet restore TAPRIM/TAPrim.csproj

# Copy toàn bộ source code
COPY . .
WORKDIR /src/TAPRIM
RUN dotnet build TAPRIM.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TAPRIM.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage (runtime)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TAPrim.dll"]
