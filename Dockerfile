FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app

COPY *.sln .
COPY NotificationApi.csproj .

RUN dotnet restore *.sln

COPY . .

WORKDIR /app
RUN dotnet publish -c Release -o /release

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=80
COPY --from=build /release ./
ENTRYPOINT ["dotnet", "NotificationApi.dll"]
