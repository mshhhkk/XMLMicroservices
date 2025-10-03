# -------- build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# сначала csproj, чтобы кешировался restore
COPY Shared/Shared.csproj Shared/
COPY FileParserService/FileParserService.csproj FileParserService/
RUN dotnet restore FileParserService/FileParserService.csproj

# теперь весь код
COPY . .
RUN dotnet publish FileParserService/FileParserService.csproj -c Release -o /out

# -------- runtime --------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "FileParserService.dll"]
