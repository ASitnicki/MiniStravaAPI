FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MiniStrava.csproj", "."]
RUN dotnet restore "./MiniStrava.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./MiniStrava.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MiniStrava.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY db-init ./db-init
ENTRYPOINT ["dotnet", "MiniStrava.dll"]
