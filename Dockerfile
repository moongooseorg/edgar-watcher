FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/*.csproj src/
COPY lib/OpenBaoConfiguration/*.csproj lib/OpenBaoConfiguration/
RUN dotnet restore src/edgar-watcher.csproj
COPY src/ src/
COPY lib/ lib/
RUN dotnet publish src/edgar-watcher.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "edgar-watcher.dll"]
