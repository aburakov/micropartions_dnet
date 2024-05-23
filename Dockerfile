FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /docker-web-api-micropartions

EXPOSE 8080

COPY . ./

RUN dotnet restore 

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /docker-web-api-micropartions
COPY --from=build-env /docker-web-api-micropartions/out .
ENTRYPOINT ["dotnet", "Micropartions.dll"]