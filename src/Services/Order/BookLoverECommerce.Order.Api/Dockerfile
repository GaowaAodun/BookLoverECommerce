FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Central Package Management
COPY ["Directory.Packages.props", "./"]

# Order projects
COPY ["src/Services/Order/BookLoverECommerce.Order.Api/BookLoverECommerce.Order.Api.csproj","src/Services/Order/BookLoverECommerce.Order.Api/"]

COPY ["src/Services/Order/BookLoverECommerce.Order.Application/BookLoverECommerce.Order.Application.csproj","src/Services/Order/BookLoverECommerce.Order.Application/"]

COPY ["src/Services/Order/BookLoverECommerce.Order.Domain/BookLoverECommerce.Order.Domain.csproj","src/Services/Order/BookLoverECommerce.Order.Domain/"]

COPY ["src/Services/Order/BookLoverECommerce.Order.Infrastructure/BookLoverECommerce.Order.Infrastructure.csproj","src/Services/Order/BookLoverECommerce.Order.Infrastructure/"]

# Shared Contracts project
COPY ["src/BuildingBlocks/BookLoverECommerce.Contracts/BookLoverECommerce.Contracts.csproj","src/BuildingBlocks/BookLoverECommerce.Contracts/"]

# Restore
RUN dotnet restore \
    "src/Services/Order/BookLoverECommerce.Order.Api/BookLoverECommerce.Order.Api.csproj"

# Copy remaining source
COPY . .

WORKDIR "/src/src/Services/Order/BookLoverECommerce.Order.Api"

RUN dotnet publish \
    "BookLoverECommerce.Order.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BookLoverECommerce.Order.Api.dll"]