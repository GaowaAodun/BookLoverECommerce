FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    src/Services/Price/BookLoverECommerce.Price.Api/BookLoverECommerce.Price.Api.csproj

RUN dotnet publish \
    src/Services/Price/BookLoverECommerce.Price.Api/BookLoverECommerce.Price.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookLoverECommerce.Price.Api.dll"]