# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy API csproj (Dockerfile is inside Golden.Publication.Api, so parent is repo root)
COPY ./Golden.Publication.Api/Golden.Publication.Api.csproj ./Golden.Publication.Api/
COPY ./src/Golden.Publication.Data/Golden.Publication.Data.csproj ./src/Golden.Publication.Data/

# Restore only API project
RUN dotnet restore Golden.Publication.Api/Golden.Publication.Api.csproj
RUN dotnet restore src/Golden.Publication.Data/Golden.Publication.Data.csproj

# Copy entire repo (repo root)
COPY . .

# Publish API
WORKDIR /src/Golden.Publication.Api
RUN dotnet publish Golden.Publication.Api.csproj -c Release -o /app/publish

# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

COPY src/Golden.Publication.Data/Data/publications.xml /app/Data/publications.xml

ENV PUBLICATION_XML_FILE=/app/Data/publications.xml
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Golden.Publication.Api.dll"]