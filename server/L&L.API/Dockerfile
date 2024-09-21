#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["L&L.API/L&L.API.csproj", "L&L.API/"]
COPY ["L&L.Business/L&L.Business.csproj", "L&L.Business/"]
COPY ["L&L.Data/L&L.Data.csproj", "L&L.Data/"]
RUN dotnet restore "./L&L.API/L&L.API.csproj"
COPY . .
WORKDIR "/src/L&L.API"
RUN dotnet build "./L&L.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./L&L.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "L&L.API.dll"]