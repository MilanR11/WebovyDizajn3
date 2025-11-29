FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Bind Kestrel to 0.0.0.0:8080 inside the container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# Run as non-root default user present in the image
USER app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MakerslabInventory/MakerslabInventory.csproj", "MakerslabInventory/"]
RUN dotnet restore "MakerslabInventory/MakerslabInventory.csproj"
COPY . .
WORKDIR "/src/MakerslabInventory"
RUN dotnet build "./MakerslabInventory.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MakerslabInventory.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MakerslabInventory.dll"]
