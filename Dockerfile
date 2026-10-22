# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy nuget.config so restore uses only nuget.org (ignores private feeds)
COPY nuget.config ./

# Copy the project files first (better layer caching on restore).
COPY src/Core/FIAP.Domain/FIAP.Domain.csproj               src/Core/FIAP.Domain/
COPY src/Core/FIAP.Application/FIAP.Application.csproj      src/Core/FIAP.Application/
COPY src/Core/FIAp.Exception/FIAp.Exception.csproj         src/Core/FIAp.Exception/
COPY src/infra/FIAP.Infrastructure/FIAP.Infrastructure.csproj src/infra/FIAP.Infrastructure/
COPY src/presentation/FIAP.UsersAPI/FIAP.UsersAPI.csproj   src/presentation/FIAP.UsersAPI/
RUN dotnet restore src/presentation/FIAP.UsersAPI/FIAP.UsersAPI.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish src/presentation/FIAP.UsersAPI/FIAP.UsersAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Kestrel listens on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FIAP.UsersAPI.dll"]
