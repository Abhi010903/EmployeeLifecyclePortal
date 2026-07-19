# Multi-stage build for .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["EmployeeLifecyclePortal.sln", "."]
COPY ["EmployeeLifecyclePortal.Api/EmployeeLifecyclePortal.Api.csproj", "EmployeeLifecyclePortal.Api/"]
COPY ["EmployeeLifecyclePortal.Application/EmployeeLifecyclePortal.Application.csproj", "EmployeeLifecyclePortal.Application/"]
COPY ["EmployeeLifecyclePortal.Domain/EmployeeLifecyclePortal.Domain.csproj", "EmployeeLifecyclePortal.Domain/"]
COPY ["EmployeeLifecyclePortal.Infrastructure/EmployeeLifecyclePortal.Infrastructure.csproj", "EmployeeLifecyclePortal.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "EmployeeLifecyclePortal.sln"

# Copy source
COPY . .

# Build
WORKDIR "/src/EmployeeLifecyclePortal.Api"
RUN dotnet build "EmployeeLifecyclePortal.Api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "EmployeeLifecyclePortal.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 80 443

# Create non-root user
RUN useradd -m -u 1000 appuser

# Copy from build
COPY --from=build /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app

USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/api/health || exit 1

ENTRYPOINT ["dotnet", "EmployeeLifecyclePortal.Api.dll"]
