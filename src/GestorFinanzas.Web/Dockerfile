FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GestorFinanzas.Web/GestorFinanzas.Web.csproj", "GestorFinanzas.Web/"]
RUN dotnet restore "GestorFinanzas.Web/GestorFinanzas.Web.csproj"
COPY . .
WORKDIR "/src/GestorFinanzas.Web"
RUN dotnet build "GestorFinanzas.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GestorFinanzas.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GestorFinanzas.Web.dll"]