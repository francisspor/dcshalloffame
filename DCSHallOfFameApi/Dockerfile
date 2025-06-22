# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["DCSHallOfFameApi.csproj", "./"]
RUN dotnet restore "DCSHallOfFameApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "DCSHallOfFameApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DCSHallOfFameApi.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for Cloud Run
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DCSHallOfFameApi.dll"]