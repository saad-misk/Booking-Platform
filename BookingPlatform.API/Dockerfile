# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build environment using the .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for API and its dependencies.
COPY ["BookingPlatform.API/BookingPlatform.API.csproj", "BookingPlatform.API/"]
COPY ["BookingPlatform.Application/BookingPlatform.Application.csproj", "BookingPlatform.Application/"]
COPY ["BookingPlatform.Domain/BookingPlatform.Domain.csproj", "BookingPlatform.Domain/"]
COPY ["BookingPlatform.Infrastructure/BookingPlatform.Infrastructure.csproj", "BookingPlatform.Infrastructure/"]

# Restore the NuGet packages
RUN dotnet restore "BookingPlatform.API/BookingPlatform.API.csproj"

# Copy the remaining source code to the container
COPY . .

# Change directory to the API project folder and build the project
WORKDIR "/src/BookingPlatform.API"
RUN dotnet build "BookingPlatform.API.csproj" -c Release -o /app/build

# Stage 3: Publish the application
FROM build AS publish
RUN dotnet publish "BookingPlatform.API.csproj" -c Release -o /app/publish

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingPlatform.API.dll"]