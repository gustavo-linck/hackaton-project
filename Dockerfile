FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/UmbLink.Web/UmbLink.Web.csproj", "src/UmbLink.Web/"]
COPY ["src/UmbLink.Application/UmbLink.Application.csproj", "src/UmbLink.Application/"]
COPY ["src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj", "src/UmbLink.Infrastructure/"]
RUN dotnet restore "src/UmbLink.Web/UmbLink.Web.csproj"
COPY . .
RUN dotnet publish "src/UmbLink.Web/UmbLink.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UmbLink.Web.dll"]
