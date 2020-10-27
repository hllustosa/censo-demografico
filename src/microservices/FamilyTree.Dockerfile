FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# Copy everything else and build
COPY ./ ./
RUN dotnet restore ./FamilyTree/Census.FamilyTree.Api
RUN dotnet publish -c Release -o /app/out ./FamilyTree/Census.FamilyTree.Api

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/out .
ARG prod
RUN if [ $prod != ""  ]; then mv prod.appsettings.json appsettings.json ; fi
ENTRYPOINT ["dotnet", "Census.FamilyTree.Api.dll"]