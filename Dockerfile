# syntax=docker/dockerfile:1

# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY *.csproj ./

RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o /app/publish --no-restore


# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/playwright/dotnet:v1.63.0-noble AS runtime

WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

ENTRYPOINT ["dotnet", "JobHunter.dll"]
