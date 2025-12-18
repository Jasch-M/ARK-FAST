FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ./src/ArkaZilla/ArkaZilla.csproj ./src/ArkaZilla/ArkaZilla.csproj
COPY ./src/ArkaZilla.Data/ArkaZilla.Data.csproj ./src/ArkaZilla.Data/ArkaZilla.Data.csproj

RUN dotnet restore ./src/ArkaZilla/ArkaZilla.csproj

COPY . .

RUN dotnet build ./src/ArkaZilla/ArkaZilla.csproj -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS prod

WORKDIR /app
COPY --from=build /app/build .

CMD ["dotnet", "/app/ArkaZilla.dll"]

