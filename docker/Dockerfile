FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/WarehouseInventory.Web/WarehouseInventory.Web.csproj", "src/WarehouseInventory.Web/"]
COPY ["src/WarehouseInventory.Domain/WarehouseInventory.Domain.csproj", "src/WarehouseInventory.Domain/"]
COPY ["src/WarehouseInventory.Application/WarehouseInventory.Application.csproj", "src/WarehouseInventory.Application/"]
COPY ["src/WarehouseInventory.Infrastructure/WarehouseInventory.Infrastructure.csproj", "src/WarehouseInventory.Infrastructure/"]
RUN dotnet restore "src/WarehouseInventory.Web/WarehouseInventory.Web.csproj"
COPY . .
WORKDIR "/src/src/WarehouseInventory.Web"
RUN dotnet build "WarehouseInventory.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WarehouseInventory.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WarehouseInventory.Web.dll"]
