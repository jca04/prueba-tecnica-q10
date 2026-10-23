# ---------------- BUILD ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/OrderFlow.Contracts/OrderFlow.Contracts.csproj             src/OrderFlow.Contracts/
COPY src/OrderFlow.InventoryWorker/OrderFlow.InventoryWorker.csproj src/OrderFlow.InventoryWorker/
RUN dotnet restore src/OrderFlow.InventoryWorker/OrderFlow.InventoryWorker.csproj

COPY src/OrderFlow.Contracts/         src/OrderFlow.Contracts/
COPY src/OrderFlow.InventoryWorker/   src/OrderFlow.InventoryWorker/
RUN dotnet publish src/OrderFlow.InventoryWorker/OrderFlow.InventoryWorker.csproj \
    -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.InventoryWorker.dll"]