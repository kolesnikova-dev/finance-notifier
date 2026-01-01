FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only needed projects
COPY Src/Core/*.csproj Src/Core/
COPY Src/Web/*.csproj Src/Web/

# Restore Web project directly (not the solution)
RUN dotnet restore Src/Web/FinanceScraper.Web.csproj

COPY Src/Core/ Src/Core/
COPY Src/Web/ Src/Web/

RUN dotnet publish Src/Web/FinanceScraper.Web.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceScraper.Web.dll"]