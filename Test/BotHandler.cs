using Boa.TelegramBotService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Test;

public class BotHandler1 : ITelegramBotHandler
{
    public int Order => 1000;

    private readonly Guid _id;

    public BotHandler1()
    {
        _id = Guid.NewGuid();
    }

    public virtual Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(ToString() + ": " + _id);
        return Task.FromResult(false);
    }
}

public class BotHandler2 : BotHandler1
{
}

public class BotHandler3 : BotHandler1
{
}


public class BotHandler4 : BotHandler1
{
    #region ITelegramBotHandler
    public override async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await base.HandleUpdateAsync(botClient, update, cancellationToken);

        return update.Type switch
        {
            UpdateType.Message => update.Message != null && await OnMessage(botClient, update.Message),
            UpdateType.CallbackQuery => update.CallbackQuery != null && await OnCallbackQuery(botClient, update.CallbackQuery),
            _ => false,
        };
    }



    public static async Task<bool> OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQuery(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}"
        );

        if (callbackQuery.Message != null)
        {
            await botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}"
            );
        }

        return true;
    }

    public static async Task<bool> OnMessage(ITelegramBotClient botClient, Message message)
    {
        if (message == null || message.Type != MessageType.Text || message.Text == null)
            return false;

        return message.Text.Split(' ').First() switch
        {
            "/inline" => await SendInlineKeyboard(botClient, message),
            "/keyboard" => await SendReplyKeyboard(botClient, message),
            //"/photo" => await SendDocument(message);
            "/request" => await RequestContactAndLocation(botClient, message),
            "/password" => await ResetPassword(botClient, message),
            _ => false,
        };
    }

    #endregion

    // Send inline keyboard
    // You can process responses in BotOnCallbackQueryReceived handler
    private static async Task<bool> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatAction(message.Chat.Id, ChatAction.Typing);

        // Simulate longer running task
        await Task.Delay(500);

        var inlineKeyboard = new InlineKeyboardMarkup([
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("1.1", "11"),
                InlineKeyboardButton.WithCallbackData("1.2", "12"),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("2.1", "21"),
                InlineKeyboardButton.WithCallbackData("2.2", "22"),
            }
        ]);

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Choose",
            replyMarkup: inlineKeyboard
        );

        return true;
    }

    private static async Task<bool> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup([
            ["1.1", "1.2"],
            ["2.1", "2.2"],
        ]);

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Choose",
            replyMarkup: replyKeyboardMarkup

        );

        return true;
    }

    //private async Task SendDocument(Message message)
    //{
    //    await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

    //    const string filePath = @"Files/tux.png";
    //    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
    //    {
    //        var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
    //        await _botClient.SendPhotoAsync(
    //            chatId: message.Chat.Id,
    //            photo: new InputOnlineFile(fileStream, fileName),
    //            caption: "Nice Picture"
    //        );
    //    }
    //}

    private static async Task<bool> RequestContactAndLocation(ITelegramBotClient botClient, Message message)
    {
        var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            KeyboardButton.WithRequestLocation("Location"),
            KeyboardButton.WithRequestContact("Contact"),
        });

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Who or Where are you?",
            replyMarkup: RequestReplyKeyboard
        );

        return true;
    }

    private static async Task<bool> ResetPassword(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Reply to this message with new password",
            replyMarkup: new ForceReplyMarkup()
        );

        return true;
    }
}
