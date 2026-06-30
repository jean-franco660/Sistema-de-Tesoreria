# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los .csproj para aprovechar la caché de restauración
COPY ["HoracioTesoreria.sln", "./"]
COPY ["src/Horacio.Domain/Horacio.Domain.csproj", "src/Horacio.Domain/"]
COPY ["src/Horacio.Application/Horacio.Application.csproj", "src/Horacio.Application/"]
COPY ["src/Horacio.Infrastructure/Horacio.Infrastructure.csproj", "src/Horacio.Infrastructure/"]
COPY ["src/Horacio.Persistence/Horacio.Persistence.csproj", "src/Horacio.Persistence/"]
COPY ["src/Horacio.API/Horacio.API.csproj", "src/Horacio.API/"]
RUN dotnet restore "src/Horacio.API/Horacio.API.csproj"

# Copiar el resto y publicar
COPY . .
RUN dotnet publish "src/Horacio.API/Horacio.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Horacio.API.dll"]
