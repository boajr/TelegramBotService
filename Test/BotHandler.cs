using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Test;

public class BotHandler : IUpdateHandler
{
    private readonly ILogger<BotHandler> _logger;

    public UpdateType[] AllowedUpdates { get; set; }

    public BotHandler(ILogger<BotHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region ITelegramBotHandler
    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string ErrorMessage = exception switch
        {
            //ApiRequestException apiRequestException => apiRequestException.Message,
            ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.Message,
        };

        _logger.LogError("{Message}", ErrorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await OnUpdate(update);

            switch (update.Type)
            {
                case UpdateType.Message:
                    await OnMessage(botClient, update.Message);
                    break;

                case UpdateType.CallbackQuery:
                    await OnCallbackQuery(botClient, update.CallbackQuery);
                    break;
            }
        }
        catch (Exception exception)
        {
            await HandlePollingErrorAsync(botClient, exception, cancellationToken);
        }
    }



    public static async Task OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}"
        );

        await botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: $"Received {callbackQuery.Data}"
        );
    }

    public static async Task OnMessage(ITelegramBotClient botClient, Message message)
    {
        if (message == null || message.Type != MessageType.Text)
            return;

        switch (message.Text.Split(' ').First())
        {
            // Send inline keyboard
            case "/inline":
                await SendInlineKeyboard(botClient, message);
                break;

            // send custom keyboard
            case "/keyboard":
                await SendReplyKeyboard(botClient, message);
                break;

            // send a photo
            //case "/photo":
            //    await SendDocument(message);
            //    break;

            // request location or contact
            case "/request":
                await RequestContactAndLocation(botClient, message);
                break;

            case "/password":
                await ResetPassword(botClient, message);
                break;

            default:
                await Usage(botClient, message);
                break;
        }
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
    private static async Task SendInlineKeyboard(ITelegramBotClient botClient, Message message)
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
    }

    private static async Task SendReplyKeyboard(ITelegramBotClient botClient, Message message)
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

    private static async Task RequestContactAndLocation(ITelegramBotClient botClient, Message message)
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
    }

    private static async Task ResetPassword(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Reply to this message with new password",
            replyMarkup: new ForceReplyMarkup()
        );
    }

    private static async Task Usage(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine(message.ReplyToMessage?.MessageId);



        const string usage = "Usage:\n" +
                                "/inline   - send inline keyboard\n" +
                                "/keyboard - send custom keyboard\n" +
                                //"/photo    - send a photo\n" +
                                "/request  - request location or contact\n" +
                                "/password  - request a reset password";
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove()
        );
    }
}
