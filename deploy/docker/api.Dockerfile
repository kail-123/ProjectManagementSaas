# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ProjectManagementSaaS.sln ./
COPY src/backend/ProjectManagementSaaS.Api/ProjectManagementSaaS.Api.csproj src/backend/ProjectManagementSaaS.Api/
COPY src/backend/ProjectManagementSaaS.Application/ProjectManagementSaaS.Application.csproj src/backend/ProjectManagementSaaS.Application/
COPY src/backend/ProjectManagementSaaS.Domain/ProjectManagementSaaS.Domain.csproj src/backend/ProjectManagementSaaS.Domain/
COPY src/backend/ProjectManagementSaaS.Infrastructure/ProjectManagementSaaS.Infrastructure.csproj src/backend/ProjectManagementSaaS.Infrastructure/

RUN dotnet restore ProjectManagementSaaS.sln

COPY src/backend src/backend

RUN dotnet publish src/backend/ProjectManagementSaaS.Api/ProjectManagementSaaS.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProjectManagementSaaS.Api.dll"]
