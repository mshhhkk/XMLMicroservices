FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Shared/Shared.csproj Shared/
COPY DataProcessorService/DataProcessorService.csproj DataProcessorService/
RUN dotnet restore DataProcessorService/DataProcessorService.csproj
COPY . .
RUN dotnet publish DataProcessorService/DataProcessorService.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "DataProcessorService.dll"]
