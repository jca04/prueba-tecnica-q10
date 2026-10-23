#------------- BUILD --------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copio  los .csproj para cachear el restore
COPY Directory.Build.props ./
COPY src/OrderFlow.Contracts/OrderFlow.Contracts.csproj     src/OrderFlow.Contracts/
COPY src/OrderFlow.OrdersApi/OrderFlow.OrdersApi.csproj     src/OrderFlow.OrdersApi/
RUN dotnet restore src/OrderFlow.OrdersApi/OrderFlow.OrdersApi.csproj

# código fuente
COPY src/OrderFlow.Contracts/  src/OrderFlow.Contracts/
COPY src/OrderFlow.OrdersApi/  src/OrderFlow.OrdersApi/
RUN dotnet publish src/OrderFlow.OrdersApi/OrderFlow.OrdersApi.csproj \
    -c Release -o /app/publish --no-restore -p:UseAppHost=false

# -------RUNTIME ------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderFlow.OrdersApi.dll"]