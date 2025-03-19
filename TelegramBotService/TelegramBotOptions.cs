namespace Boa.TelegramBotService;

/// <summary>
/// Options for configure telegram bot handler.
/// </summary>
public class TelegramBotOptions
{
    /// <summary>
    /// The token used to authenticate the Bot on the Telegram platform.
    /// </summary>
    public string BotToken { get; set; } = default!;

    /// <summary>
    /// Used to change base URL to your private Bot API server URL. It looks like http://localhost:8081. Path, query and fragment will be omitted if present.
    /// </summary>
    public string? BotBaseUrl { get; set; } = default;

    /// <summary>
    /// Indicates that test environment will be used.
    /// </summary>
    public bool BotUseTestEnvironment { get; set; } = false;

    /// <summary>
    /// A custom <see cref="HttpClient"/> to use with Bot.
    /// </summary>
    public HttpClient? BotHttpClient { get; set; }

    /// <summary>
    /// Automatic retry of failed requests "Too Many Requests: retry after X" when X is less or equal to RetryThreshold.
    /// </summary>
    public int BotRetryThreshold { get; set; } = 60;

    /// <summary>
    /// <see cref="RetryThreshold">Automatic retry</see> will be attempted for up to RetryCount requests
    /// </summary>
    public int BotRetryCount { get; set; } = 3;
}
