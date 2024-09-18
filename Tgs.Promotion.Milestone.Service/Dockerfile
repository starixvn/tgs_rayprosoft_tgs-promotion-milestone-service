#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tgs.Promotion.Milestone.Service/Tgs.Promotion.Milestone.Service.csproj", "Tgs.Promotion.Milestone.Service/"]
COPY ["Tgs.Promotion.Milestone.Core/Tgs.Promotion.Milestone.Core.csproj", "Tgs.Promotion.Milestone.Core/"]
COPY ["Tgs.Promotion.Milestone.Domain/Tgs.Promotion.Milestone.Domain.csproj", "Tgs.Promotion.Milestone.Domain/"]
COPY ["Tgs.Promotion.Milestone.Repository/Tgs.Promotion.Milestone.Repository.csproj", "Tgs.Promotion.Milestone.Repository/"]
RUN dotnet restore "./Tgs.Promotion.Milestone.Service/Tgs.Promotion.Milestone.Service.csproj"
COPY . .
WORKDIR "/src/Tgs.Promotion.Milestone.Service"
RUN dotnet build "./Tgs.Promotion.Milestone.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.Promotion.Milestone.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Promotion.Milestone.Service.dll"]