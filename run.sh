#!/bin/bash

# Discord Bot Launcher для GTA 5 Семьи
# Скрипт для запуска бота на Linux/Mac

echo
echo "============================================================"
echo "      Discord Bot для GTA 5 Семьи - Запуск (Linux/Mac)"
echo "============================================================"
echo

# Проверяем наличие .NET
if ! command -v dotnet &> /dev/null; then
    echo "[✗] Ошибка: .NET не установлен!"
    echo "Скачайте .NET 6.0 с https://dotnet.microsoft.com/download"
    read -p "Нажмите Enter для выхода"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "[✓] .NET найден: $DOTNET_VERSION"
echo

# Проверяем наличие файла конфигурации
if [ ! -f "config.json" ]; then
    echo "[✗] Ошибка: Файл config.json не найден!"
    echo "Пожалуйста, создайте файл config.json и заполните его данные."
    read -p "Нажмите Enter для выхода"
    exit 1
fi

echo "[✓] config.json найден"
echo

# Восстанавливаем зависимости
echo "Восстановление зависимостей..."
dotnet restore > /dev/null

if [ $? -ne 0 ]; then
    echo "[✗] Ошибка при восстановлении зависимостей!"
    read -p "Нажмите Enter для выхода"
    exit 1
fi

echo "[✓] Зависимости восстановлены"
echo

# Запускаем бота
echo "Запуск бота..."
echo "============================================================"
echo

dotnet run --no-restore

echo
echo "============================================================"
echo "Бот остановлен. Нажмите Enter для выхода..."
read
