# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["IoT-System.Api/IoT-System.Api.csproj", "IoT-System.Api/"]
COPY ["IoT-System.Application/IoT-System.Application.csproj", "IoT-System.Application/"]
COPY ["IoT-System.Domain/IoT-System.Domain.csproj", "IoT-System.Domain/"]
COPY ["IoT-System.Infrastructure/IoT-System.Infrastructure.csproj", "IoT-System.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "IoT-System.Api/IoT-System.Api.csproj"

# Copy all source files
COPY . .

# Build project
WORKDIR "/src/IoT-System.Api"
RUN dotnet build "IoT-System.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "IoT-System.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "IoT-System.Api.dll"]