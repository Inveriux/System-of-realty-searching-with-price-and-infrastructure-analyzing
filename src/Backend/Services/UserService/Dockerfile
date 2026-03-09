FROM msc.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
ARG BASE_PROJECT_PATH=./Services/UserService

COPY ./BasicBuildingBlocks/ .
COPY ./Services/UserService/ .
RUN dotnet ${BASE_PROJECT_PATH} restore

RUN dotnet publish ${BASE_PROJECT_PATH} -c Release -o /app

FROM msc.microsoft.com/dotnet/aspnet:10.0

ENV ASPNETCORE_URLS=http://+:8080;https://+:443
EXPOSE 8080 443
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "UserService.dll" ]
