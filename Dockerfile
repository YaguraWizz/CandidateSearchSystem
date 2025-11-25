# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем solution и весь проект
COPY CandidateSearchSystem.sln ./
COPY CandidateSearchSystem/ ./CandidateSearchSystem/

# Восстанавливаем зависимости по solution
RUN dotnet restore CandidateSearchSystem.sln

# Публикуем проект (Blazor Server/Server проект)
RUN dotnet publish CandidateSearchSystem.sln -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Проброс портов
EXPOSE 8080

# Запуск приложения (замените на нужный dll)
ENTRYPOINT ["dotnet", "CandidateSearchSystem.dll"]
