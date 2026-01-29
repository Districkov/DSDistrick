using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using DiscordBot.Models;

namespace DiscordBot.Modules
{
    public class ButtonModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ApplicationService _applicationService;
        private readonly AutoService _autoService;
        private readonly LoggerService _loggerService;
        private readonly BotConfig _config;
        private readonly DiscordSocketClient _client;

        public ButtonModule(ApplicationService applicationService, AutoService autoService, LoggerService loggerService, BotConfig config, DiscordSocketClient client)
        {
            _applicationService = applicationService;
            _autoService = autoService;
            _loggerService = loggerService;
            _config = config;
            _client = client;
        }

        [ComponentInteraction("approve_*")]
        public async Task ApproveApplication(string userId)
        {
            if (!ulong.TryParse(userId, out var uId))
            {
                await RespondAsync("Ошибка при обработке заявки.", ephemeral: true);
                return;
            }

            _applicationService.UpdateApplicationStatus(uId, "Approved");
            await _loggerService.LogEventAsync("ApplicationApproved", $"Заявка пользователя {uId} была одобрена", Context.User.Id, Context.User.Username);

            var embed = new EmbedBuilder()
                .WithTitle("✅ Заявка одобрена")
                .WithDescription($"Поздравляем! Ваша заявка была одобрена.")
                .WithColor(Color.Green)
                .Build();

            try
            {
                var user = await _client.GetUserAsync(uId);
                if (user != null)
                {
                    await user.SendMessageAsync(embed: embed);
                }
            }
            catch { }

            await RespondAsync("✅ Заявка одобрена!", ephemeral: true);
        }

        [ComponentInteraction("reject_*")]
        public async Task RejectApplication(string userId)
        {
            if (!ulong.TryParse(userId, out var uId))
            {
                await RespondAsync("Ошибка при обработке заявки.", ephemeral: true);
                return;
            }

            _applicationService.UpdateApplicationStatus(uId, "Rejected");
            await _loggerService.LogEventAsync("ApplicationRejected", $"Заявка пользователя {uId} была отклонена", Context.User.Id, Context.User.Username);

            var embed = new EmbedBuilder()
                .WithTitle("❌ Заявка отклонена")
                .WithDescription($"К сожалению, ваша заявка была отклонена.")
                .WithColor(Color.Red)
                .Build();

            try
            {
                var user = await _client.GetUserAsync(uId);
                if (user != null)
                {
                    await user.SendMessageAsync(embed: embed);
                }
            }
            catch { }

            await RespondAsync("❌ Заявка отклонена!", ephemeral: true);
        }

        [ComponentInteraction("admin_approve_*")]
        public async Task AdminApproveApplication(string userId)
        {
            // Проверка прав администратора
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту кнопку.", ephemeral: true);
                return;
            }

            if (!ulong.TryParse(userId, out var uId))
            {
                await RespondAsync("Ошибка при обработке заявки.", ephemeral: true);
                return;
            }

            _applicationService.UpdateApplicationStatus(uId, "Approved");
            await _loggerService.LogEventAsync("AdminApproval", $"Администратор {Context.User.Username} одобрил заявку пользователя {uId}", Context.User.Id, Context.User.Username);
            await RespondAsync($"✅ Заявка пользователя <@{uId}> одобрена!", ephemeral: true);
        }

        [ComponentInteraction("admin_reject_*")]
        public async Task AdminRejectApplication(string userId)
        {
            // Проверка прав администратора
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ Только администраторы могут использовать эту кнопку.", ephemeral: true);
                return;
            }

            if (!ulong.TryParse(userId, out var uId))
            {
                await RespondAsync("Ошибка при обработке заявки.", ephemeral: true);
                return;
            }

            _applicationService.UpdateApplicationStatus(uId, "Rejected");
            await _loggerService.LogEventAsync("AdminRejection", $"Администратор {Context.User.Username} отклонил заявку пользователя {uId}", Context.User.Id, Context.User.Username);
            await RespondAsync($"❌ Заявка пользователя <@{uId}> отклонена!", ephemeral: true);
        }

        [ComponentInteraction("family_apply_btn")]
        public async Task FamilyApplyButton()
        {
            var modal = new ModalBuilder()
                .WithTitle("👨‍👩‍👧‍👦 Заявка в семью")
                .WithCustomId($"application_modal")
                .AddTextInput("Ваш ник в игре", "ingame_nick", placeholder: "Например: JohnDoe_123", required: true, maxLength: 50)
                .AddTextInput("Ваше полное имя", "full_name", placeholder: "Ваше реальное имя", required: true, maxLength: 100)
                .AddTextInput("Ваш онлайн статус", "online_status", placeholder: "Например: Активен в вечерние часы", required: true, maxLength: 100)
                .AddTextInput("Расскажите о себе", "about", TextInputStyle.Paragraph, placeholder: "Расскажите о себе и вашем игровом опыте", required: true, maxLength: 1024)
                .AddTextInput("Почему вы хотите присоединиться?", "reason", TextInputStyle.Paragraph, placeholder: "Ваше желание присоединиться к нашей семье", required: true, maxLength: 1024);

            await Context.Interaction.RespondWithModalAsync(modal.Build());
        }

        [ComponentInteraction("auto_select_btn")]
        public async Task AutoSelectButton()
        {
            Console.WriteLine($"🔔 Кнопка auto_select_btn нажата");
            Console.WriteLine($"   Пользователь: {Context.User.Username}");
            
            // Получаем список машин
            var cars = _autoService.GetAllCars();
            
            if (cars == null || cars.Count == 0)
            {
                await RespondAsync("❌ В базе нет машин!", ephemeral: true);
                return;
            }
            
            // Создаем эмбед со списком машин
            var embed = new EmbedBuilder()
                .WithTitle("🚗 Выберите машину для поездки")
                .WithColor(Color.Purple)
                .WithDescription("Выберите нужную машину из списка ниже:")
                .WithFooter("После выбора укажите пункт назначения и причину");
            
            // Добавляем список машин в эмбед
            var machineList = "";
            foreach (var car in cars)
            {
                var status = car.IsOccupied ? "❌" : "✅";
                var owner = car.IsOccupied ? $" ({car.OccupiedByName})" : "";
                machineList += $"{status} **{car.Name}**{owner}\n";
            }
            embed.AddField("Доступные машины", machineList);
            
            // Создаем select меню для выбора машины
            var selectMenu = new SelectMenuBuilder()
                .WithPlaceholder("Выберите машину")
                .WithCustomId($"car_select_for_request_{Context.User.Id}")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach (var car in cars)
            {
                var status = car.IsOccupied ? "❌ Занята" : "✅ Свободна";
                var owner = car.IsOccupied ? $" ({car.OccupiedByName})" : "";
                selectMenu.AddOption(
                    label: $"{car.Name}",
                    value: car.Name,
                    description: $"{status}{owner}"
                );
            }
            
            var component = new ComponentBuilder()
                .WithSelectMenu(selectMenu)
                .Build();
            
            await RespondAsync(embed: embed.Build(), components: component, ephemeral: true);
            
            Console.WriteLine($"   ✅ Список машин отправлен");
        }
    }
}
