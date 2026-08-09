FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Packages.props", "./"]
COPY ["src/Web/BookLoverECommerce.Cms/BookLoverECommerce.Cms.csproj", "src/Web/BookLoverECommerce.Cms/"]

RUN dotnet restore \
    "src/Web/BookLoverECommerce.Cms/BookLoverECommerce.Cms.csproj"

COPY . .

WORKDIR "/src/src/Web/BookLoverECommerce.Cms"

RUN dotnet publish \
    "BookLoverECommerce.Cms.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookLoverECommerce.Cms.dll"]