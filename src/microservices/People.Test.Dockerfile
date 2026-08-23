FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./ ./
RUN dotnet test ./People/Census.People.Test/Census.People.Test.csproj -c Release --no-restore || dotnet restore ./People/Census.People.Test && dotnet test ./People/Census.People.Test/Census.People.Test.csproj -c Release
