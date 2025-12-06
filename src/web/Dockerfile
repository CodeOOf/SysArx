FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/web/SysArx.csproj", "src/web/"]
RUN dotnet restore "src/web/SysArx.csproj"

COPY . .
WORKDIR "/src/src/web"
RUN dotnet build "SysArx.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SysArx.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SysArx.dll"]
