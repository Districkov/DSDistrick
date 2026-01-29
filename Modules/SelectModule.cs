using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using DiscordBot.Models;

namespace DiscordBot.Modules
{
    public class SelectModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AutoService _autoService;
        private readonly LoggerService _loggerService;
        private readonly BotConfig _config;
        private readonly DiscordSocketClient _client;

        public SelectModule(AutoService autoService, LoggerService loggerService, BotConfig config, DiscordSocketClient client)
        {
            _autoService = autoService;
            _loggerService = loggerService;
            _config = config;
            _client = client;
        }

        [ComponentInteraction("car_select_for_request_*")]
        public async Task HandleCarSelectForRequest(string selectedCar)
        {
            Console.WriteLine($"🔔 SelectInteraction car_select_for_request получен");
            Console.WriteLine($"   Пользователь: {Context.User.Username} ({Context.User.Id})");
            Console.WriteLine($"   Выбранная машина: {selectedCar}");
            
            try
            {
                // Открываем модаль для ввода пункта назначения и причины
                var modal = new ModalBuilder()
                    .WithTitle("🚗 Запрос машины")
                    .WithCustomId($"car_request_with_car_{selectedCar}_{Context.User.Id}")
                    .AddTextInput("Пункт назначения", "destination", placeholder: "Куда вам нужна машина?", required: true, maxLength: 100)
                    .AddTextInput("Причина", "reason", TextInputStyle.Paragraph, placeholder: "Зачем вам нужна машина?", required: true, maxLength: 500);

                await Context.Interaction.RespondWithModalAsync(modal.Build());
                
                Console.WriteLine($"   ✅ Модаль открыта для заполнения");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в HandleCarSelectForRequest: {ex.Message}");
                try
                {
                    await RespondAsync($"❌ Ошибка: {ex.Message}", ephemeral: true);
                }
                catch { }
            }
        }

        [ComponentInteraction("car_select_final_*")]
        public async Task HandleCarSelection(string selectedValue)
        {
            Console.WriteLine($"🔔 SelectInteraction car_select_final получен");
            Console.WriteLine($"   Пользователь: {Context.User.Username} ({Context.User.Id})");
            Console.WriteLine($"   Значение: {selectedValue}");
            
            try
            {
                if (string.IsNullOrEmpty(selectedValue))
                {
                    await RespondAsync("❌ Ошибка при выборе машины.", ephemeral: true);
                    return;
                }

                // Парсим значение: машина|назначение|причина
                var parts = selectedValue.Split('|');
                if (parts.Length < 1)
                {
                    await RespondAsync("❌ Ошибка при обработке выбора.", ephemeral: true);
                    return;
                }
                
                var selectedCar = parts[0];
                var destination = parts.Length > 1 ? parts[1] : "неизвестно";
                var reason = parts.Length > 2 ? parts[2] : "не указана";

                _autoService.SelectCar(Context.User.Id, Context.User.Username, selectedCar);
                await _loggerService.LogEventAsync("AutoSelection", $"{Context.User.Username} выбрал машину {selectedCar} для поездки в {destination}", Context.User.Id, Context.User.Username);

                // Отправляем уведомление в канал выбора авто
                var autoChannel = _client.GetChannel(_config.TransportRequestChannelId) as IMessageChannel;
                if (autoChannel != null)
                {
                    var embed = new EmbedBuilder()
                        .WithTitle("🚗 Новый запрос машины")
                        .WithDescription($"**{Context.User.Username}** выбрал(а) машину для поездки")
                        .AddField("🚗 Машина", selectedCar, inline: true)
                        .AddField("📍 Пункт назначения", destination, inline: true)
                        .AddField("📝 Причина", reason)
                        .WithColor(Color.Purple)
                        .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                        .WithTimestamp(DateTime.UtcNow)
                        .Build();

                    await autoChannel.SendMessageAsync(embed: embed);
                    Console.WriteLine($"   ✅ Уведомление отправлено в канал");
                }
                else
                {
                    Console.WriteLine($"   ⚠️  Канал выбора авто не найден! ID: {_config.TransportRequestChannelId}");
                }

                // Показываем обновленный статус машин
                var cars = _autoService.GetAllCars();
                var available = _autoService.GetAvailableCars();
                var occupied = _autoService.GetOccupiedCars();

                var statusEmbed = new EmbedBuilder()
                    .WithTitle("✅ Машина зарезервирована!")
                    .WithDescription($"Вы выбрали машину **{selectedCar}** для поездки в **{destination}**")
                    .AddField("📊 Текущий статус машин", 
                        $"✅ Свободных: {available.Count}\n❌ Занятых: {occupied.Count}", 
                        inline: false)
                    .WithColor(Color.Green)
                    .WithTimestamp(DateTime.UtcNow)
                    .Build();

                await RespondAsync(embed: statusEmbed, ephemeral: true);
                Console.WriteLine($"   ✅ Выбор успешно обработан");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в HandleCarSelection: {ex.Message}");
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
