using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using DiscordBot.Models;

namespace DiscordBot.Modules
{
    public class ModalModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ApplicationService _applicationService;
        private readonly AutoService _autoService;
        private readonly LoggerService _loggerService;
        private readonly BotConfig _config;
        private readonly DiscordSocketClient _client;

        public ModalModule(
            ApplicationService applicationService,
            AutoService autoService,
            LoggerService loggerService,
            BotConfig config,
            DiscordSocketClient client)
        {
            _applicationService = applicationService;
            _autoService = autoService;
            _loggerService = loggerService;
            _config = config;
            _client = client;
        }

        [ModalInteraction("car_request_modal*", ignoreGroupNames: true)]
        public async Task HandleCarRequest(string destination, string reason)
        {
            Console.WriteLine($"🔔 ModalInteraction car_request_modal получен");
            Console.WriteLine($"   Пользователь: {Context.User.Username} ({Context.User.Id})");
            Console.WriteLine($"   Пункт назначения: {destination}");
            Console.WriteLine($"   Причина: {reason}");
            
            try
            {
                if (string.IsNullOrWhiteSpace(destination) || string.IsNullOrWhiteSpace(reason))
                {
                    await RespondAsync("❌ Ошибка: все поля должны быть заполнены!", ephemeral: true);
                    return;
                }

                var cars = _autoService.GetAllCars();
                
                if (cars == null || cars.Count == 0)
                {
                    await RespondAsync("❌ В базе нет доступных машин", ephemeral: true);
                    Console.WriteLine("⚠️  Ошибка: список машин пуст");
                    return;
                }

                var selectMenu = new SelectMenuBuilder()
                    .WithPlaceholder("Выберите машину")
                    .WithCustomId($"car_select_{Context.User.Id}")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                foreach (var car in cars)
                {
                    var status = car.IsOccupied ? "❌" : "✅";
                    selectMenu.AddOption(
                        label: $"{status} {car.Name}",
                        value: $"{car.Name}|{destination}|{reason}",
                        description: car.IsOccupied ? $"Занята: {car.OccupiedByName}" : "Свободна"
                    );
                }

                var component = new ComponentBuilder()
                    .WithSelectMenu(selectMenu)
                    .Build();

                await RespondAsync(
                    $"🚗 Выбрали поездку в **{destination}**\nПричина: _{reason}_\n\nВыберите машину:",
                    components: component,
                    ephemeral: true
                );

                await _loggerService.LogEventAsync(
                    "CarRequest",
                    $"Пользователь {Context.User.Username} запросил машину в {destination}",
                    Context.User.Id,
                    Context.User.Username
                );
                
                Console.WriteLine($"   ✅ Модаль успешно обработана");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в HandleCarRequest: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                try
                {
                    await RespondAsync($"❌ Ошибка: {ex.Message}", ephemeral: true);
                }
                catch { }
            }
        }

        [ModalInteraction("application_modal*", ignoreGroupNames: true)]
        public async Task HandleApplication(string ingame_nick, string full_name, string online_status, string about, string reason)
        {
            Console.WriteLine($"🔔 ModalInteraction application_modal получен");
            Console.WriteLine($"   Пользователь: {Context.User.Username} ({Context.User.Id})");
            Console.WriteLine($"   Ник в игре: {ingame_nick}");
            
            try
            {
                if (string.IsNullOrWhiteSpace(ingame_nick) || string.IsNullOrWhiteSpace(full_name))
                {
                    await RespondAsync("❌ Ошибка: ник и имя обязательны!", ephemeral: true);
                    return;
                }

                var app = new FamilyApplication
                {
                    UserId = Context.User.Id,
                    Username = Context.User.Username,
                    NickName = ingame_nick,
                    FullName = full_name,
                    OnlineStatus = online_status,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    Message = $"**Ник:** {ingame_nick}\n**Имя:** {full_name}\n**Онлайн:** {online_status}\n**О себе:** {about}\n**Причина:** {reason}"
                };

                _applicationService.AddApplication(app);

                // Отправляем в канал заявок
                var channel = _client.GetChannel(_config.FamilyApplicationsChannelId) as IMessageChannel;
                if (channel != null)
                {
                    var embed = new EmbedBuilder()
                        .WithTitle("👨‍👩‍👧‍👦 Новая заявка")
                        .WithDescription($"От: {Context.User.Mention}")
                        .AddField("🎮 Ник в игре", ingame_nick, inline: true)
                        .AddField("👤 Имя", full_name, inline: true)
                        .AddField("🟢 Онлайн", online_status, inline: true)
                        .AddField("📝 О себе", about)
                        .AddField("❓ Причина", reason)
                        .WithColor(Color.Green)
                        .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                        .WithTimestamp(DateTime.UtcNow)
                        .Build();

                    var buttons = new ComponentBuilder()
                        .WithButton("✅ Одобрить", $"approve_{Context.User.Id}", ButtonStyle.Success)
                        .WithButton("❌ Отклонить", $"reject_{Context.User.Id}", ButtonStyle.Danger)
                        .Build();

                    await channel.SendMessageAsync(embed: embed, components: buttons);
                    Console.WriteLine($"   ✅ Заявка отправлена в канал");
                }
                else
                {
                    Console.WriteLine($"   ⚠️  Канал заявок не найден! ID: {_config.FamilyApplicationsChannelId}");
                }

                await RespondAsync(
                    "✅ Заявка отправлена! Ожидайте ответа администраторов.",
                    ephemeral: true
                );

                await _loggerService.LogEventAsync(
                    "FamilyApplication",
                    $"Новая заявка от {Context.User.Username} (ник: {ingame_nick})",
                    Context.User.Id,
                    Context.User.Username
                );
                
                Console.WriteLine($"   ✅ Модаль успешно обработана");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в HandleApplication: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                try
                {
                    await RespondAsync($"❌ Ошибка: {ex.Message}", ephemeral: true);
                }
                catch { }
            }
        }
    }
}
