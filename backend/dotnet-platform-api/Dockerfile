# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for restore-layer caching
COPY AiShop.Platform.sln .
COPY AiShop.Platform.Api/*.csproj            AiShop.Platform.Api/
COPY AiShop.Platform.Application/*.csproj    AiShop.Platform.Application/
COPY AiShop.Platform.Domain/*.csproj         AiShop.Platform.Domain/
COPY AiShop.Platform.Infrastructure/*.csproj AiShop.Platform.Infrastructure/
RUN dotnet restore AiShop.Platform.Api/AiShop.Platform.Api.csproj

COPY . .
RUN dotnet publish AiShop.Platform.Api/AiShop.Platform.Api.csproj -c Release -o /app /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AiShop.Platform.Api.dll"]
