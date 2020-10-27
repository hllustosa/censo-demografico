FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /testapp

# Copy everything else and test
COPY ./ ./
ENTRYPOINT ["dotnet", "test", "./FamilyTree/Census.FamilyTree.Test/Census.FamilyTree.Test.csproj"]