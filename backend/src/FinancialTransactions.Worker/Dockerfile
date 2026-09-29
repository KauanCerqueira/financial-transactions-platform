FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FinancialTransactions.Domain/FinancialTransactions.Domain.csproj src/FinancialTransactions.Domain/
COPY src/FinancialTransactions.Application/FinancialTransactions.Application.csproj src/FinancialTransactions.Application/
COPY src/FinancialTransactions.Infrastructure/FinancialTransactions.Infrastructure.csproj src/FinancialTransactions.Infrastructure/
COPY src/FinancialTransactions.Worker/FinancialTransactions.Worker.csproj src/FinancialTransactions.Worker/
RUN dotnet restore src/FinancialTransactions.Worker/FinancialTransactions.Worker.csproj

COPY src/ src/
RUN dotnet publish src/FinancialTransactions.Worker/FinancialTransactions.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FinancialTransactions.Worker.dll"]
