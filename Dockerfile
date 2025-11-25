# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем csproj и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем весь проект и собираем
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Экспонируем порты
EXPOSE 7232
EXPOSE 5150

# Запуск приложения
ENTRYPOINT ["dotnet", "YourProjectName.dll"]
