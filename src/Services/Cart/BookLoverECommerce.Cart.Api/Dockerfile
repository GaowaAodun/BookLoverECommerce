FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    src/Services/Cart/BookLoverECommerce.Cart.Api/BookLoverECommerce.Cart.Api.csproj

RUN dotnet publish \
    src/Services/Cart/BookLoverECommerce.Cart.Api/BookLoverECommerce.Cart.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookLoverECommerce.Cart.Api.dll"]