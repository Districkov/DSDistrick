# Fly.io Development & Deployment Guide

## Установка Fly.io CLI

```bash
# Windows (Chocolatey)
choco install flyctl

# или прямой скачивание
# https://fly.io/docs/getting-started/installing-flyctl/
```

## Развертывание на Fly.io

1. **Авторизация:**
```bash
fly auth login
```

2. **Инициализация приложения:**
```bash
fly launch
# Следуйте подсказкам, выберите регион
```

3. **Установка секрета (токен бота):**
```bash
fly secrets set DISCORD_BOT_TOKEN=<ваш_токен>
```
Замените `<ваш_токен>` на настоящий токен из Discord Developer Portal.

4. **Развертывание:**
```bash
fly deploy
```

5. **Просмотр логов:**
```bash
fly logs
```

6. **Проверка статуса:**
```bash
fly status
```

## Проверка переменных окружения

```bash
fly secrets list  # Посмотреть установленные секреты
```

## Обновление токена

```bash
fly secrets set DISCORD_BOT_TOKEN=<новый_токен>
fly deploy  # Перезаразвернуть приложение
```

## Удаление приложения

```bash
fly apps delete <app-name>
```
