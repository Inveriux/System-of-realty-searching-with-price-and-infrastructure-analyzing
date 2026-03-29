FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
ARG BASE_PROJECT_PATH=./Services/PricingAnalyticsService/PricingAnalyticsService.csproj

COPY ./BasicBuildingBlocks ./BasicBuildingBlocks
COPY ./Services/PricingAnalyticsService ./Services/PricingAnalyticsService
RUN dotnet restore ${BASE_PROJECT_PATH}

RUN dotnet publish ${BASE_PROJECT_PATH} -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080 443
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "PricingAnalyticsService.dll" ]
