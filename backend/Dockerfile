FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FoodPower/FoodPower.csproj FoodPower/
RUN dotnet restore FoodPower/FoodPower.csproj

COPY . .
RUN dotnet publish FoodPower/FoodPower.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Render injects PORT; default to 8080 for local runs.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet FoodPower.dll"]
