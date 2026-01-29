# Discord Bot Launcher для GTA 5 Семьи
# Скрипт для запуска бота

Write-Host "`n" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "    Discord Bot для GTA 5 Семьи - Запуск (PowerShell)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "`n"

# Проверяем наличие .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "[✓] .NET найден: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "[✗] Ошибка: .NET не установлен!" -ForegroundColor Red
    Write-Host "Скачайте .NET 6.0 с https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Read-Host "Нажмите Enter для выхода"
    exit 1
}

Write-Host "`n"

# Проверяем наличие файла конфигурации
if (-not (Test-Path "config.json")) {
    Write-Host "[✗] Ошибка: Файл config.json не найден!" -ForegroundColor Red
    Write-Host "Пожалуйста, создайте файл config.json и заполните его данные." -ForegroundColor Yellow
    Read-Host "Нажмите Enter для выхода"
    exit 1
}

Write-Host "[✓] config.json найден" -ForegroundColor Green
Write-Host "`n"

# Восстанавливаем зависимости
Write-Host "Восстановление зависимостей..." -ForegroundColor Yellow
dotnet restore | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "[✗] Ошибка при восстановлении зависимостей!" -ForegroundColor Red
    Read-Host "Нажмите Enter для выхода"
    exit 1
}

Write-Host "[✓] Зависимости восстановлены" -ForegroundColor Green
Write-Host "`n"

# Запускаем бота
Write-Host "Запуск бота..." -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "`n"

dotnet run --no-restore

Write-Host "`n"
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Бот остановлен. Нажмите Enter для выхода..." -ForegroundColor Yellow
Read-Host
