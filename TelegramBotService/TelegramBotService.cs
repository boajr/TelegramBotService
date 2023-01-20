using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram
{
    public class TelegramBotService<THandler> : TelegramBotClient, IHostedService, IDisposable
        where THandler : IUpdateHandler
    {
        private readonly THandler _handler;
        private readonly ILogger<TelegramBotService<THandler>> _logger;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public TelegramBotService(THandler handler, IOptions<TelegramBotOptions<THandler>> options, ILogger<TelegramBotService<THandler>> logger)
            : base(options.Value.BotToken, options.Value.BotHttpClient)
        {
            _handler = handler != null ? handler : throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
                };
                if (!cancellationToken.IsCancellationRequested)
                    this.StartReceiving(_handler, receiverOptions, _stoppingCts.Token);

                User me = await this.GetMeAsync(cancellationToken);
                _logger.LogInformation($"Connected as user {me.Username} (botId: {me.Id})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (cancellationToken.IsCancellationRequested)
                _stoppingCts.Cancel();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service is shutting down...");
            _stoppingCts.Cancel();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
