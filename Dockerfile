FROM mcr.microsoft.com/dotnet/aspnet:9.0 as base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DapperIdentity.Web/DapperIdentity.Web.csproj", "DapperIdentity.Web/"]
COPY ["DapperIdentity.Models/DapperIdentity.Models.csproj", "DapperIdentity.Models/"]
COPY ["DapperIdentity.Configuration/DapperIdentity.Configuration.csproj", "DapperIdentity.Configuration/"]
RUN dotnet restore "DapperIdentity.Web/DapperIdentity.Web.csproj"
COPY . .
WORKDIR "/src/DapperIdentity.Web"
RUN dotnet build "DapperIdentity.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build as publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DapperIdentity.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish -p:UseAppHost=false

FROM base as final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DapperIdentity.Web.dll"]