FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Directory.Packages.props", "./"]

COPY ["src/Services/Cart/BookLoverECommerce.Cart.Api/BookLoverECommerce.Cart.Api.csproj","src/Services/Cart/BookLoverECommerce.Cart.Api/"]

COPY ["src/Services/Cart/BookLoverECommerce.Cart.Application/BookLoverECommerce.Cart.Application.csproj","src/Services/Cart/BookLoverECommerce.Cart.Application/"]

COPY ["src/Services/Cart/BookLoverECommerce.Cart.Domain/BookLoverECommerce.Cart.Domain.csproj","src/Services/Cart/BookLoverECommerce.Cart.Domain/"]

COPY ["src/Services/Cart/BookLoverECommerce.Cart.Infrastructure/BookLoverECommerce.Cart.Infrastructure.csproj","src/Services/Cart/BookLoverECommerce.Cart.Infrastructure/"]

COPY ["src/BuildingBlocks/BookLoverECommerce.Contracts/BookLoverECommerce.Contracts.csproj","src/BuildingBlocks/BookLoverECommerce.Contracts/"]

RUN dotnet restore \
    "src/Services/Cart/BookLoverECommerce.Cart.Api/BookLoverECommerce.Cart.Api.csproj"

COPY . .

WORKDIR "/src/src/Services/Cart/BookLoverECommerce.Cart.Api"

RUN dotnet publish \
    "BookLoverECommerce.Cart.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BookLoverECommerce.Cart.Api.dll"]