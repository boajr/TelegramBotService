using Telegram.Bot.Polling;

namespace Telegram
{
    /// <summary>
    /// Options for configure telegram bot handler.
    /// </summary>
    public class TelegramBotOptions<THandler> where THandler : IUpdateHandler
    {
        public string BotToken { get; set; }

        public HttpClient BotHttpClient { get; set; }
    }
}
