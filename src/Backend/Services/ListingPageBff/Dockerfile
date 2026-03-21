FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
ARG BASE_PROJECT_PATH=./Services/ListingPageBff/ListingPageBff.csproj

COPY ./BasicBuildingBlocks ./BasicBuildingBlocks
COPY ./Services/ListingPageBff ./Services/ListingPageBff
RUN dotnet restore ${BASE_PROJECT_PATH}

RUN dotnet publish ${BASE_PROJECT_PATH} -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080 443
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "ListingPageBff.dll" ]
