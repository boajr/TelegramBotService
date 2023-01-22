using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotService;

namespace Test;

public class BotHandler : ITelegramBotHandler
{
    public int Order => 1000;

    private readonly ILogger<BotHandler> _logger;

    public BotHandler(ILogger<BotHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region ITelegramBotHandler
    public async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine("BotHandler");

        await OnUpdate(update);

        switch (update.Type)
        {
            case UpdateType.Message:
                return update.Message != null && await OnMessage(botClient, update.Message);

            case UpdateType.CallbackQuery:
                return update.CallbackQuery != null && await OnCallbackQuery(botClient, update.CallbackQuery);
        }
        return false;
    }



    public static async Task<bool> OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}"
        );

        if (callbackQuery.Message != null)
        {
            await botClient.SendTextMessageAsync(
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

    public Task OnUpdate(Update update)
    {
        //Update e = eventArgs.Update;

        _logger.LogInformation("OnUpdate: {Type}", update.Type);
        return Task.CompletedTask;
    }
    #endregion

    // Send inline keyboard
    // You can process responses in BotOnCallbackQueryReceived handler
    private static async Task<bool> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        // Simulate longer running task
        await Task.Delay(500);

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
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
            });

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Choose",
            replyMarkup: inlineKeyboard
        );

        return true;
    }

    private static async Task<bool> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "1.1", "1.2" },
                    new KeyboardButton[] { "2.1", "2.2" },
            }//,
            //resizeKeyboard: true
        );

        await botClient.SendTextMessageAsync(
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

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Who or Where are you?",
            replyMarkup: RequestReplyKeyboard
        );

        return true;
    }

    private static async Task<bool> ResetPassword(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Reply to this message with new password",
            replyMarkup: new ForceReplyMarkup()
        );

        return true;
    }
}
