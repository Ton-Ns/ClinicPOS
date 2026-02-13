# Use the official .NET 10 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY ClinicPOS.API/*.csproj ./ClinicPOS.API/
COPY ClinicPOS.Domain/*.csproj ./ClinicPOS.Domain/
COPY ClinicPOS.Infrastructure/*.csproj ./ClinicPOS.Infrastructure/
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish ClinicPOS.API/ClinicPOS.API.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "ClinicPOS.API.dll"]
