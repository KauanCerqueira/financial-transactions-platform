FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FinancialTransactions.Domain/FinancialTransactions.Domain.csproj src/FinancialTransactions.Domain/
COPY src/FinancialTransactions.Application/FinancialTransactions.Application.csproj src/FinancialTransactions.Application/
COPY src/FinancialTransactions.Infrastructure/FinancialTransactions.Infrastructure.csproj src/FinancialTransactions.Infrastructure/
COPY src/FinancialTransactions.Api/FinancialTransactions.Api.csproj src/FinancialTransactions.Api/
RUN dotnet restore src/FinancialTransactions.Api/FinancialTransactions.Api.csproj

COPY src/ src/
RUN dotnet publish src/FinancialTransactions.Api/FinancialTransactions.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FinancialTransactions.Api.dll"]
