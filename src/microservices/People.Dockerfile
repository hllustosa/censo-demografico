FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# Copy everything else and build
COPY ./ ./
RUN dotnet restore ./People/Census.People.Api
RUN dotnet publish -c Release -o /app/out ./People/Census.People.Api

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/out .
RUN mv appsettings.Development.json appsettings.json
ENTRYPOINT ["dotnet", "Census.People.Api.dll"]