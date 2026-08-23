FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./ ./
RUN dotnet test ./FamilyTree/Census.FamilyTree.Test/Census.FamilyTree.Test.csproj -c Release --no-restore || dotnet restore ./FamilyTree/Census.FamilyTree.Test && dotnet test ./FamilyTree/Census.FamilyTree.Test/Census.FamilyTree.Test.csproj -c Release
