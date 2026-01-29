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

        [ComponentInteraction("car_select_final_*")]
        public async Task HandleCarSelection(string selectedValue)
        {
            if (string.IsNullOrEmpty(selectedValue))
            {
                await RespondAsync("❌ Ошибка при выборе машины.", ephemeral: true);
                return;
            }

            // Парсим значение: машина|назначение|причина
            var parts = selectedValue.Split('|');
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
        }
    }
}
