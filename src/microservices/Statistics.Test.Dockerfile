FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /testapp

# Copy everything else and test
COPY ./ ./
ENTRYPOINT ["dotnet", "test", "./Statistics/Census.Statistics.Test/Census.Statistics.Test.csproj"]