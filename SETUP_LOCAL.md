# Локальная настройка Discord бота

## Шаг 1: Получить токен бота

1. Перейдите на [Discord Developer Portal](https://discord.com/developers/applications)
2. Нажмите **"New Application"** (или выберите существующее приложение)
3. Откройте вкладку **"Bot"** слева
4. Нажмите **"Add Bot"**
5. В разделе **TOKEN** нажмите **"Copy"** - это ваш токен
6. **НИКОГДА** не делитесь этим токеном!

## Шаг 2: Настроить config.json

Откройте файл `config.json` и установите:

```json
{
  "BotToken": "ВАШ_ТОКЕН_ЗДЕСЬ",
  "ApplicationId": "ID_ПРИЛОЖЕНИЯ"
}
```

Где взять `ApplicationId`:
- В Discord Developer Portal откройте **"General Information"**
- Скопируйте **"Application ID"**

## Шаг 3: Включить интенты

В Discord Developer Portal на вкладке **"Bot"** включите:
- ✅ **Message Content Intent** (обязательно!)
- ✅ **Server Members Intent**
- ✅ **Presence Intent**

## Шаг 4: Добавить бота на сервер

1. На вкладке **"OAuth2"** → **"URL Generator"**
2. Выберите скопы:
   - ✅ bot
   - ✅ applications.commands
3. Выберите разрешения:
   - ✅ Send Messages
   - ✅ Manage Messages
   - ✅ Embed Links
   - ✅ Use Application Commands
4. Скопируйте сгенерированный URL и откройте его в браузере

## Шаг 5: Запустить бота локально

### Windows (PowerShell):
```powershell
dotnet run --project .
```

### Linux/Mac:
```bash
dotnet run --project .
```

## Шаг 6: Проверка

В консоли должны появиться логи:

```
⚙️  Конфигурация:
   BotToken: ***ПОСЛЕДНИЕ_10_СИМВОЛОВ
   ApplicationId: 1466372211689525399

🔐 Попытка подключения к Discord...
   ✅ Токен загружен из переменной окружения DISCORD_BOT_TOKEN

✅ Бот {BotName}#1234 готов!
   Статус: Connected
```

## Что если не работает?

### ❌ "BotToken не установлен"
- Проверьте, что `config.json` содержит правильный токен
- Убедитесь, что файл находится в корне проекта

### ❌ "Message Content Intent"
- Включите **Message Content Intent** в Developer Portal
- Это обязательно для обработки модалей и компонентов

### ❌ "Бот не отвечает на команды"
- Проверьте, что бот добавлен на сервер
- Убедитесь, что он имеет права на отправку сообщений в канале
- Проверьте консоль на ошибки (смотрите логи в коде)

### ❌ "Модали не срабатывают"
- Проверьте консоль на ошибки обработчика (смотрите `🔔 ModalInteraction...`)
- Убедитесь, что `ignoreGroupNames: true` установлен в атрибуте `[ModalInteraction]`

## Переменные окружения (опционально)

Вместо config.json вы можете использовать переменные окружения:

```powershell
$env:DISCORD_BOT_TOKEN = "ВАШ_ТОКЕН"
dotnet run
```

На Render/Fly.io эти переменные должны быть установлены в Dashboard.

## Структура проекта

```
📦 DiscordBot
├── 📄 Program.cs           - Главный файл, регистрация модулей
├── 📄 config.json          - Конфигурация (токен, ID каналов)
├── 📁 Modules/             - Обработчики команд и компонентов
│   ├── ModalModule.cs      - Обработка модалей (диалоговых окон)
│   ├── SelectModule.cs     - Обработка выпадающих меню
│   └── ButtonModule.cs     - Обработка кнопок
├── 📁 Services/            - Вспомогательные сервисы
│   ├── ConfigService.cs    - Загрузка конфигурации
│   ├── LoggerService.cs    - Логирование событий
│   └── ...
└── 📁 Models/              - Модели данных
```

## Где смотреть ошибки?

1. **Консоль** - основная информация и ошибки
2. **Discord Logs** - канал `server-logs` (если настроен)
3. **Стек ошибок** - выводится в консоль при исключениях
