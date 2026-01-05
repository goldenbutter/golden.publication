# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution context (API + src)
COPY ./Microchip.Interview.Api ./Microchip.Interview.Api
COPY ./src ./src

# Restore & publish
RUN dotnet restore ./Microchip.Interview.Api/Microchip.Interview.Api.csproj
RUN dotnet publish ./Microchip.Interview.Api/Microchip.Interview.Api.csproj -c Release -o /app/publish

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy the XML into the image and point env to it
COPY ./src/Microchip.Interview.Data/Data/publications.xml /app/Data/publications.xml
ENV PUBLICATION_XML_FILE=/app/Data/publications.xml

# Expose HTTP
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Microchip.Interview.Api.dll"]
