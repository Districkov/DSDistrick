# Развертывание на Render

## Шаги для развертывания Discord бота на Render:

### 1. Подготовка репозитория
```bash
git add .
git commit -m "Prepare for Render deployment"
git push origin main
```

### 2. Создание приложения на Render
1. Перейдите на [https://dashboard.render.com](https://dashboard.render.com)
2. Нажмите **"New +"** → **"Web Service"**
3. Выберите **"Build and deploy from a Git repository"**
4. Подключите ваш GitHub репозиторий (если нужно авторизоваться)
5. Выберите репозиторий **DSDistrick**

### 3. Конфигурация сервиса
- **Name**: `discord-bot` (или другое уникальное имя)
- **Environment**: Docker
- **Branch**: `main` (или ваша текущая ветка)
- **Build Command**: (оставить пусто - будет использоваться Dockerfile)
- **Start Command**: (оставить пусто)

### 4. Переменные окружения
Нажмите **"Advanced"** и добавьте переменные:
- **Key**: `DISCORD_BOT_TOKEN`
- **Value**: Вставьте ваш Discord токен бота

### 5. Запуск развертывания
Нажмите **"Create Web Service"** и ждите завершения развертывания (~5-10 минут)

## Примечания

- **Dockerfile** уже готов для Render
- **render.yaml** автоматизирует конфигурацию
- Бот будет слушать на порту **8080** (как указано в вашем коде)
- Логи можно смотреть в реальном времени в панели Render

## Если развертывание не удается

1. Проверьте логи в Render Dashboard
2. Убедитесь, что токен бота установлен правильно
3. Проверьте, что `config.json` скопирован в контейнер (он есть в Dockerfile)

## Остановка приложения

Если нужно остановить бота:
1. Перейдите в Render Dashboard
2. Выберите сервис
3. Нажмите **"Suspend"** или **"Delete"**
