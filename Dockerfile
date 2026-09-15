FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["FinanceTracker.Api/FinanceTracker.Api.csproj", "FinanceTracker.Api/"]
RUN dotnet restore "FinanceTracker.Api/FinanceTracker.Api.csproj"
COPY . .
WORKDIR "/src/FinanceTracker.Api"
RUN dotnet build "FinanceTracker.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceTracker.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceTracker.Api.dll"]