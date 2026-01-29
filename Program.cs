using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Services;
using DiscordBot.Models;

// Получаем путь к корню проекта
var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "config.json");
var config = new ConfigService(configPath).GetConfig();

var client = new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds 
                   | GatewayIntents.GuildMessages 
                   | GatewayIntents.DirectMessages 
                   | GatewayIntents.MessageContent
});

Console.WriteLine($"⚙️  Конфигурация:");
Console.WriteLine($"   BotToken: {(string.IsNullOrEmpty(config.BotToken) ? "[ПУСТО]" : "***" + config.BotToken.Substring(config.BotToken.Length - 10))}");
Console.WriteLine($"   ApplicationId: {config.ApplicationId}");

var interactionService = new InteractionService(client);

var services = new ServiceCollection()
    .AddSingleton(client)
    .AddSingleton(interactionService)
    .AddSingleton(config)
    .AddSingleton<ApplicationService>()
    .AddSingleton<AutoService>()
    .AddSingleton<TransportService>()
    .AddSingleton<LoggerService>()
    .AddSingleton<SocialMediaService>()
    .BuildServiceProvider();

client.Ready += ReadyAsync;
client.InteractionCreated += InteractionCreatedAsync;
client.UserJoined += UserJoinedAsync;
client.MessageReceived += MessageReceivedAsync;
client.Connected += ConnectedAsync;
client.Disconnected += DisconnectedAsync;

async Task ConnectedAsync()
{
    Console.WriteLine("✅ Бот подключился к WebSocket!");
    return;
}

async Task DisconnectedAsync(Exception? ex)
{
    Console.WriteLine($"\n❌ Бот отключился!");
    if (ex != null)
    {
        Console.WriteLine($"   Причина: {ex.Message}");
        Console.WriteLine($"   Тип: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Внутренняя ошибка: {ex.InnerException.Message}");
        }
    }
    
    Console.WriteLine($"\n⚠️  ПРОВЕРЬТЕ:");
    Console.WriteLine($"   1. Правильность ApplicationId в config.json");
    Console.WriteLine($"   2. Правильность BotToken в config.json");
    Console.WriteLine($"   3. Что Message Content Intent включён в Developer Portal");
    Console.WriteLine($"   4. Что бот добавлен на сервер");
    
    // Пытаемся переподключиться через 5 секунд
    Console.WriteLine($"\n🔄 Попытка переподключения через 5 секунд...");
    await Task.Delay(5000);
    try
    {
        await client.StartAsync();
    }
    catch (Exception reconnectEx)
    {
        Console.WriteLine($"❌ Ошибка при переподключении: {reconnectEx.Message}");
    }
    return;
}

async Task ReadyAsync()
{
    Console.WriteLine($"✅ Бот {client.CurrentUser} готов!");
    Console.WriteLine($"   Статус: {client.ConnectionState}");
    
    try
    {
        var loggerService = services.GetRequiredService<LoggerService>();
        var serverLogsChannel = client.GetChannel(config.ServerLogsChannelId) as IMessageChannel;
        
        if (serverLogsChannel != null)
        {
            loggerService.SetLogChannel(serverLogsChannel);
            await loggerService.LogEventAsync("BotStartup", $"Бот запущен успешно в {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine("✅ Логирование подключено!");
        }
        else
        {
            Console.WriteLine($"⚠️  Канал логов не найден! ID: {config.ServerLogsChannelId}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Ошибка при подготовке логирования: {ex.Message}");
    }
    
    try
    {
        Console.WriteLine("🧹 Удаление старых команд...");
        
        // Удаляем команды со всех серверов
        foreach (var guild in client.Guilds)
        {
            try
            {
                var guildCommands = await guild.GetApplicationCommandsAsync();
                Console.WriteLine($"   Сервер '{guild.Name}': найдено {guildCommands.Count} команд");
                
                foreach (var cmd in guildCommands)
                {
                    await cmd.DeleteAsync();
                    Console.WriteLine($"      ✓ Удалена команда: {cmd.Name}");
                }
            }
            catch (Exception guildEx)
            {
                Console.WriteLine($"   ⚠️  Ошибка на сервере '{guild.Name}': {guildEx.Message}");
            }
        }
        
        // Удаляем глобальные команды
        var globalCommands = await client.Rest.GetGlobalApplicationCommands();
        Console.WriteLine($"   Глобальные команды: {globalCommands.Count()}");
        foreach (var cmd in globalCommands)
        {
            try
            {
                await cmd.DeleteAsync();
                Console.WriteLine($"      ✓ Удалена глобальная команда: {cmd.Name}");
            }
            catch { }
        }
        
        await Task.Delay(2000); // Ждём перед регистрацией новых
        
        Console.WriteLine("📝 Регистрация новых команд...");
        
        // Регистрируем команды на всех серверах (быстрее для разработки)
        foreach (var guild in client.Guilds)
        {
            try
            {
                await interactionService.RegisterCommandsToGuildAsync(guild.Id);
                Console.WriteLine($"   ✓ Команды зарегистрированы на сервере '{guild.Name}'");
            }
            catch (Exception guildEx)
            {
                Console.WriteLine($"   ⚠️  Ошибка регистрации на '{guild.Name}': {guildEx.Message}");
            }
        }
        
        Console.WriteLine("✅ Команды успешно зарегистрированы!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при работе с командами: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Внутренняя ошибка: {ex.InnerException.Message}");
        }
    }
}

async Task InteractionCreatedAsync(SocketInteraction interaction)
{
    var ctx = new SocketInteractionContext(client, interaction);
    
    try
    {
        var result = await interactionService.ExecuteCommandAsync(ctx, services);
        
        if (!result.IsSuccess)
        {
            var loggerService = services.GetRequiredService<LoggerService>();
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await loggerService.LogEventAsync("CommandError", $"Условие не выполнено: {result.ErrorReason}", interaction.User.Id, interaction.User.Username);
                    Console.WriteLine($"❌ Условие не выполнено: {result.ErrorReason}");
                    break;
                case InteractionCommandError.UnknownCommand:
                    await loggerService.LogEventAsync("CommandError", $"Неизвестная команда", interaction.User.Id, interaction.User.Username);
                    Console.WriteLine($"❌ Неизвестная команда");
                    break;
                default:
                    await loggerService.LogEventAsync("CommandError", $"Ошибка: {result.ErrorReason}", interaction.User.Id, interaction.User.Username);
                    Console.WriteLine($"❌ Ошибка: {result.ErrorReason}");
                    break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Исключение при обработке взаимодействия: {ex}");
    }
}

async Task UserJoinedAsync(SocketGuildUser user)
{
    var loggerService = services.GetRequiredService<LoggerService>();
    await loggerService.LogEventAsync("UserJoined", $"Пользователь присоединился к серверу", user.Id, user.Username);
    Console.WriteLine($"👤 {user.Username} присоединился к серверу");
}

async Task MessageReceivedAsync(SocketMessage message)
{
    if (message.Author.IsBot) return;
    
    // Можно добавить дополнительную логику обработки сообщений
}

await interactionService.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), services);

Console.WriteLine("📦 Загруженные модули:");
foreach (var module in interactionService.Modules)
{
    Console.WriteLine($"   ✓ {module.Name}");
    Console.WriteLine($"      Команд: {module.SlashCommands.Count}");
    Console.WriteLine($"      Компонентов: {module.ComponentCommands.Count}");
    Console.WriteLine($"      Модалей: {module.ModalCommands.Count}");
    
    if (module.ModalCommands.Count > 0)
    {
        foreach (var modal in module.ModalCommands)
        {
            Console.WriteLine($"         - Modal: {modal.Name}");
        }
    }
}

try
{
    Console.WriteLine("\n🔐 Попытка подключения к Discord...");
    
    if (string.IsNullOrEmpty(config.BotToken) || config.BotToken == "YOUR_BOT_TOKEN_HERE")
    {
        Console.WriteLine("❌ ОШИБКА: BotToken не установлен или имеет значение по умолчанию!");
        Console.WriteLine("   Пожалуйста, установите правильный токен в config.json");
        Console.WriteLine("\n   Текущее значение токена: " + (string.IsNullOrEmpty(config.BotToken) ? "[ПУСТО]" : config.BotToken.Substring(0, Math.Min(10, config.BotToken.Length)) + "..."));
        return;
    }
    
    await client.LoginAsync(TokenType.Bot, config.BotToken);
    Console.WriteLine("✅ Авторизация успешна!");
    
    await client.StartAsync();
    Console.WriteLine("✅ Подключение к Discord выполнено!");
    
    Console.WriteLine("\n🚀 Discord бот запущен!");
    Console.WriteLine("📋 Доступные команды:");
    Console.WriteLine("  /family apply - Подать заявку в семью");
    Console.WriteLine("  /transport request - Запросить транспорт");
    Console.WriteLine("  /auto select - Выбрать машину");
    Console.WriteLine("  /admin applications - Список активных заявок");
    Console.WriteLine("  /admin transport - Список активных запросов");
    Console.WriteLine("  /admin logs - Просмотреть логи (только для админов)");
    Console.WriteLine("  /setup family-menu - Отправить меню в канал заявок");
    Console.WriteLine("  /setup auto-menu - Отправить меню выбора машин");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ОШИБКА при подключении к Discord:");
    Console.WriteLine($"   {ex.Message}");
    Console.WriteLine($"\n   Стек вызовов: {ex.StackTrace}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"\n   Внутренняя ошибка: {ex.InnerException.Message}");
    }
    
    return;
}

// Держим бота активным
await Task.Delay(Timeout.Infinite);
