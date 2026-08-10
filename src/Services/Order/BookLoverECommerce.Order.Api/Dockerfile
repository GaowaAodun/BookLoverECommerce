# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Packages.props", "./"]
# COPY ["Directory.Build.props", "./"]

COPY ["src/Services/Order/BookLoverECommerce.Order.Api/BookLoverECommerce.Order.Api.csproj", "src/Services/Order/BookLoverECommerce.Order.Api/"]
COPY ["src/Services/Order/BookLoverECommerce.Order.Application/BookLoverECommerce.Order.Application.csproj", "src/Services/Order/BookLoverECommerce.Order.Application/"]
COPY ["src/Services/Order/BookLoverECommerce.Order.Domain/BookLoverECommerce.Order.Domain.csproj", "src/Services/Order/BookLoverECommerce.Order.Domain/"]
COPY ["src/Services/Order/BookLoverECommerce.Order.Infrastructure/BookLoverECommerce.Order.Infrastructure.csproj", "src/Services/Order/BookLoverECommerce.Order.Infrastructure/"]

RUN dotnet restore \
    "src/Services/Order/BookLoverECommerce.Order.Api/BookLoverECommerce.Order.Api.csproj"

COPY . .

WORKDIR "/src/src/Services/Order/BookLoverECommerce.Order.Api"

RUN dotnet publish \
    "BookLoverECommerce.Order.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookLoverECommerce.Order.Api.dll"]