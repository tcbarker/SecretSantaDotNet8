#sudo docker build -t SecretSantaDocker .
#sudo docker run -p xxxx:8080 SecretSantaDocker
#http://localhost:xxxx/
#but no sql, so see: docker-compose


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SecretSanta/SecretSanta.csproj", "SecretSanta/"]
RUN dotnet restore "./SecretSanta/./SecretSanta.csproj"
COPY ["SecretSanta.Client/SecretSanta.Client.csproj", "SecretSanta.Client/"]
RUN dotnet restore "./SecretSanta.Client/./SecretSanta.Client.csproj"
COPY ["SecretSanta.Shared/SecretSanta.Shared.csproj", "SecretSanta.Shared/"]
RUN dotnet restore "./SecretSanta.Shared/./SecretSanta.Shared.csproj"
COPY . .

#
#WORKDIR "https://cdn.vivasoftltd.com/src/Product.API"
#RUN dotnet build "./Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SecretSanta/./SecretSanta.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecretSanta.dll"]
