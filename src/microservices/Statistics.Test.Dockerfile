FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./ ./
RUN dotnet test ./Statistics/Census.Statistics.Test/Census.Statistics.Test.csproj -c Release --no-restore || dotnet restore ./Statistics/Census.Statistics.Test && dotnet test ./Statistics/Census.Statistics.Test/Census.Statistics.Test.csproj -c Release
