using Boa.TelegramBotService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Test;

public class BotHandlerUsage : ITelegramBotHandler
{
    public int Order => 9999;

    public BotHandlerUsage() { }

    public async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine("BotHandlerUsage");

        if (update.Type != UpdateType.Message || update.Message == null)
            return false;

        string response = (update.Message.Type == MessageType.Text && update.Message.Text != null && update.Message.Text.Split(' ').First() == "/ciao")
            ? "Ciao"
            : "Usage:\n" +
                "/inline   - send inline keyboard\n" +
                "/keyboard - send custom keyboard\n" +
                "/request  - request location or contact\n" +
                "/password  - request a reset password";

        await botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: response,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return true;
    }
}
