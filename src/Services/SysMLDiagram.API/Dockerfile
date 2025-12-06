FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Healthchecks/Healthchecks.csproj", "src/BuildingBlocks/Healthchecks/"]
COPY ["src/Services/SysMLDiagram.API/SysMLDiagram.API.csproj", "src/Services/SysMLDiagram.API/"]

RUN dotnet restore "src/Services/SysMLDiagram.API/SysMLDiagram.API.csproj"

COPY . .
WORKDIR "/src/src/Services/SysMLDiagram.API"
RUN dotnet build "SysMLDiagram.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SysMLDiagram.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SysMLDiagram.API.dll"]
