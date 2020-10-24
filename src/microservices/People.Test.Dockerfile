FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /testapp

# Copy everything else and test
COPY ./ ./
ENTRYPOINT ["dotnet", "test", "./People/Census.People.Test/Census.People.Test.csproj"]