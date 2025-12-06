FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Healthchecks/Healthchecks.csproj", "src/BuildingBlocks/Healthchecks/"]
COPY ["src/Services/SysMLStore/SysMLStore.csproj", "src/Services/SysMLStore/"]

RUN dotnet restore "src/Services/SysMLStore/SysMLStore.csproj"

COPY . .
WORKDIR "/src/src/Services/SysMLStore"
RUN dotnet build "SysMLStore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SysMLStore.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SysMLStore.dll"]
