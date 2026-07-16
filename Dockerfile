# Sprint 32: Docker - Multi-stage build for production

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EmployeeLifecyclePortal.Api/EmployeeLifecyclePortal.Api.csproj", "EmployeeLifecyclePortal.Api/"]
COPY ["EmployeeLifecyclePortal.Application/EmployeeLifecyclePortal.Application.csproj", "EmployeeLifecyclePortal.Application/"]
COPY ["EmployeeLifecyclePortal.Infrastructure/EmployeeLifecyclePortal.Infrastructure.csproj", "EmployeeLifecyclePortal.Infrastructure/"]
COPY ["EmployeeLifecyclePortal.Domain/EmployeeLifecyclePortal.Domain.csproj", "EmployeeLifecyclePortal.Domain/"]
COPY ["EmployeeLifecyclePortal.Shared/EmployeeLifecyclePortal.Shared.csproj", "EmployeeLifecyclePortal.Shared/"]

RUN dotnet restore "EmployeeLifecyclePortal.Api/EmployeeLifecyclePortal.Api.csproj"

COPY . .
WORKDIR "/src/EmployeeLifecyclePortal.Api"
RUN dotnet build "EmployeeLifecyclePortal.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeLifecyclePortal.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "EmployeeLifecyclePortal.Api.dll"]
