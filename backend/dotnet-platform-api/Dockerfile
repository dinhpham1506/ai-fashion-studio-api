# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for restore-layer caching
COPY AiFashionStudio.Platform.sln .
COPY AiFashionStudio.Platform.Api/*.csproj            AiFashionStudio.Platform.Api/
COPY AiFashionStudio.Platform.Application/*.csproj    AiFashionStudio.Platform.Application/
COPY AiFashionStudio.Platform.Domain/*.csproj         AiFashionStudio.Platform.Domain/
COPY AiFashionStudio.Platform.Infrastructure/*.csproj AiFashionStudio.Platform.Infrastructure/
RUN dotnet restore AiFashionStudio.Platform.Api/AiFashionStudio.Platform.Api.csproj

COPY . .
RUN dotnet publish AiFashionStudio.Platform.Api/AiFashionStudio.Platform.Api.csproj -c Release -o /app /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AiFashionStudio.Platform.Api.dll"]
