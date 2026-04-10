# Stage 1: build + publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BloomWatch.slnx ./
COPY src/ src/

RUN dotnet restore src/BloomWatch.Api/BloomWatch.Api.csproj
RUN dotnet publish src/BloomWatch.Api/BloomWatch.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# Stage 2: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BloomWatch.Api.dll"]
