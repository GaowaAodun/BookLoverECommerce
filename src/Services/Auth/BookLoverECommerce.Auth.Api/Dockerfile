FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    src/Services/Auth/BookLoverECommerce.Auth.Api/BookLoverECommerce.Auth.Api.csproj

RUN dotnet publish \
    src/Services/Auth/BookLoverECommerce.Auth.Api/BookLoverECommerce.Auth.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookLoverECommerce.Auth.Api.dll"]