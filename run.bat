@echo off
chcp 65001 >nul
title Discord Bot - GTA 5 Family Server
cls
echo.
echo ============================================================
echo         Discord Bot для GTA 5 Семьи - Запуск
echo ============================================================
echo.

REM Проверяем наличие .NET 10
echo Проверка .NET SDK...
"C:\Program Files\dotnet\dotnet.exe" --version >nul 2>&1
if errorlevel 1 (
    echo [ОШИБКА] .NET не установлен!
    echo Скачайте .NET 10.0 SDK с https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [✓] .NET найден
echo.

REM Проверяем наличие файла конфигурации
if not exist "config.json" (
    echo [ОШИБКА] Файл config.json не найден!
    echo Пожалуйста, создайте файл config.json и заполните его данные.
    pause
    exit /b 1
)

echo [✓] config.json найден
echo.

REM Восстанавливаем зависимости
echo Восстановление зависимостей...
"C:\Program Files\dotnet\dotnet.exe" restore >nul 2>&1
if errorlevel 1 (
    echo [ОШИБКА] Ошибка при восстановлении зависимостей!
    pause
    exit /b 1
)

echo [✓] Зависимости восстановлены
echo.

REM Запускаем бота
echo Запуск бота...
echo ============================================================
echo.

"C:\Program Files\dotnet\dotnet.exe" run --no-restore

pause

echo.
echo ============================================================
echo Бот остановлен. Нажмите любую клавишу для выхода...
pause
