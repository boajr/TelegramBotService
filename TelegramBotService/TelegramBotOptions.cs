namespace TelegramBotService;

/// <summary>
/// Options for configure telegram bot handler.
/// </summary>
public class TelegramBotOptions
{
    public string BotToken { get; set; } = default!;

    public HttpClient? BotHttpClient { get; set; }
}
