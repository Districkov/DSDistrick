using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using DiscordBot.Models;

namespace DiscordBot.Modules
{
    [Group("setup", "Команды для настройки меню")]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly LoggerService _loggerService;
        private readonly BotConfig _config;

        public AdminModule(LoggerService loggerService, BotConfig config)
        {
            _loggerService = loggerService;
            _config = config;
        }

        [SlashCommand("family-menu", "Отправить меню заявок в семью")]
        public async Task SendFamilyMenu()
        {
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту команду.", ephemeral: true);
                return;
            }

            var channel = Context.Channel as IMessageChannel;
            if (channel == null)
            {
                await RespondAsync("❌ Не удается отправить сообщение в этот канал.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("👨‍👩‍👧‍👦 Заявки открыты")
                .WithColor(Color.Green)
                .WithDescription("Нажмите кнопку ниже, чтобы подать заявку в нашу семью!")
                .AddField("📋 Уведомление о приглашении", "Придет вам в личные сообщения")
                .AddField("⏱️ Сроки рассмотрения", "Заявки обрабатываются в течение 2-3 рабочих дней.")
                .AddField("ℹ️ Важно", "Пожалуйста, убедитесь, что ваш профиль доступен для связи,\nи следите за уведомлениями, чтобы не пропустить приглашение.")
                .AddField("🎯 Мы ценим ваше время", "Стараемся обрабатывать все заявки максимально быстро и внимательно.")
                .WithThumbnailUrl("https://cdn.discordapp.com/emojis/1029308030050156544.png")
                .WithFooter("GTA 5 Family Server")
                .Build();

            var button = new ComponentBuilder()
                .WithButton("Нажми на меня", "family_apply_btn", ButtonStyle.Primary, emote: new Emoji("📝"));

            await channel.SendMessageAsync(embed: embed, components: button.Build());
            await RespondAsync("✅ Меню заявок отправлено!", ephemeral: true);
            await _loggerService.LogEventAsync("MenuSent", "Меню заявок в семью отправлено", Context.User.Id, Context.User.Username);
        }

        [SlashCommand("auto-menu", "Отправить меню выбора машин и запроса транспорта")]
        public async Task SendAutoMenu()
        {
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту команду.", ephemeral: true);
                return;
            }

            var channel = Context.Channel as IMessageChannel;
            if (channel == null)
            {
                await RespondAsync("❌ Не удается отправить сообщение в этот канал.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🚗 Выбор машины и запрос транспорта")
                .WithColor(Color.Purple)
                .WithDescription("Нажмите кнопку ниже, чтобы запросить машину для поездки!")
                .AddField("✅ Свободные машины", "Список доступных автомобилей обновляется в реальном времени")
                .AddField("❌ Занятые машины", "Показывают, кто в данный момент использует автомобиль")
                .AddField("📍 Укажите пункт назначения", "После подтверждения укажите, куда вам нужна машина")
                .AddField("📝 Опишите причину", "Расскажите, зачем вам нужна машина")
                .WithThumbnailUrl("https://cdn.discordapp.com/emojis/1029308030050156544.png")
                .WithFooter("GTA 5 Family Server")
                .Build();

            var button = new ComponentBuilder()
                .WithButton("Выбрать машину", "auto_select_btn", ButtonStyle.Primary, emote: new Emoji("🚗"));

            await channel.SendMessageAsync(embed: embed, components: button.Build());
            await RespondAsync("✅ Меню машин отправлено!", ephemeral: true);
            await _loggerService.LogEventAsync("MenuSent", "Меню выбора машин отправлено", Context.User.Id, Context.User.Username);
        }

        [SlashCommand("view-logs", "Просмотреть логи сервера")]
        public async Task ViewLogs(int count = 10)
        {
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту команду.", ephemeral: true);
                return;
            }

            var logs = _loggerService.GetLogs(Math.Min(count, 25));

            if (logs.Count == 0)
            {
                await RespondAsync("📋 Логи отсутствуют.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("📋 Логи сервера")
                .WithColor(Color.Gold)
                .WithTimestamp(DateTime.UtcNow);

            foreach (var log in logs)
            {
                var userInfo = log.UserId.HasValue ? $" ({log.UserName})" : "";
                embed.AddField(
                    $"[{log.EventType}] {log.Timestamp:HH:mm:ss}",
                    log.Description + userInfo,
                    inline: false
                );
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("manage-socials", "Управление социальными сетями")]
        public async Task ManageSocials()
        {
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту команду.", ephemeral: true);
                return;
            }

            var modal = new ModalBuilder()
                .WithTitle("Настройка социальных сетей")
                .WithCustomId("socials_modal")
                .AddTextInput("Twitch Channel ID", "twitch_id", placeholder: "ID канала Twitch", required: false)
                .AddTextInput("YouTube Channel ID", "youtube_id", placeholder: "ID канала YouTube", required: false)
                .AddTextInput("TikTok Username", "tiktok_user", placeholder: "Юзернейм TikTok", required: false);

            await Context.Interaction.RespondWithModalAsync(modal.Build());
        }

        [SlashCommand("clear-commands", "🗑️ Удалить все зарегистрированные команды")]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        public async Task ClearCommands()
        {
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту команду.", ephemeral: true);
                return;
            }

            try
            {
                await DeferAsync();
                
                var guild = Context.Guild;
                var commands = await guild.GetApplicationCommandsAsync();
                
                int deleted = 0;
                foreach (var cmd in commands)
                {
                    await cmd.DeleteAsync();
                    deleted++;
                }
                
                await FollowupAsync($"✅ Удалено {deleted} команд! Перезагрузите бота для регистрации новых команд.");
                await _loggerService.LogEventAsync("CommandsClear", $"Удалено {deleted} команд администратором", Context.User.Id, Context.User.Username);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"❌ Ошибка при удалении команд: {ex.Message}", ephemeral: true);
            }
        }
    }
}
