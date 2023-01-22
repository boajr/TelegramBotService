using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotService
{
    public interface ITelegramBotHandler
    {
        int Order { get; }

        Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}
