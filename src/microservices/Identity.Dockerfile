FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./ ./
COPY Directory.Build.props Directory.Packages.props ./
RUN dotnet restore ./Identity/Census.Identity.Api
RUN dotnet publish -c Release -o /app/out ./Identity/Census.Identity.Api

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Census.Identity.Api.dll"]
