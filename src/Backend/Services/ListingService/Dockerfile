FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
ARG BASE_PROJECT_PATH=./Services/ListingService/ListingService.Presentation/ListingService.Presentation.csproj
ARG SOLUTION_PATH=./Services/ListingService/ListingService.slnx

COPY . .
RUN dotnet restore ${SOLUTION_PATH}

RUN dotnet publish ${BASE_PROJECT_PATH} -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:80;https://+:443"
EXPOSE 80 443

COPY --from=build /app ./
ENTRYPOINT [ "dotnet", "ListingService.Presentation.dll" ]