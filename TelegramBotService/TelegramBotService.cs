using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Boa.TelegramBotService;

public sealed class TelegramBotService : TelegramBotClient, IUpdateHandler, IHostedService, IDisposable
{
    private readonly List<ITelegramBotHandler> _handlers = new();
    private readonly ILogger<TelegramBotService> _logger;
    private readonly CancellationTokenSource _stoppingCts = new();

    public TelegramBotService(IOptions<TelegramBotOptions> options,
                              IEnumerable<ITelegramBotHandler> handlers,
                              ILogger<TelegramBotService> logger)
        : base(options.Value.BotToken, options.Value.BotHttpClient)
    {
        foreach (ITelegramBotHandler handler in handlers)
        {
            AddUpdateHandler(handler);
        }
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddUpdateHandler(ITelegramBotHandler handler)
    {
        int idx = _handlers.Count;
        int s = 0;
        int e = idx - 1;
        int order = handler.Order;

        while (e >= s)
        {
            idx = (s + e) / 2;
            if (_handlers[idx].Order < order)
            {
                s = ++idx;
            }
            else
            {
                e = idx - 1;
            }
        }

        if (idx >= _handlers.Count)
        {
            _handlers.Add(handler);
        }
        else
        {
            _handlers.Insert(idx, handler);
        }
    }

    public void RemoveUpdateHandler(ITelegramBotHandler handler)
    {
        _handlers.Remove(handler);
    }

    #region IUpdateHandler
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        foreach (ITelegramBotHandler handler in _handlers)
        {
            try
            {
                if (await handler.HandleUpdateAsync(botClient, update, cancellationToken))
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                await HandlePollingErrorAsync(botClient, exception, cancellationToken);
            }
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => apiRequestException.Message,
            //ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.Message,
        };

        _logger.LogError("{Message}", ErrorMessage);
        return Task.CompletedTask;
    }
    #endregion

    #region IHostedService
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
                this.StartReceiving(this, receiverOptions, _stoppingCts.Token);

            User me = await this.GetMeAsync(cancellationToken);
            _logger.LogInformation("Connected as user {Username} (botId: {Id})", me.Username, me.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("{Message}", ex.Message);
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
    #endregion

    #region IDisposable
    public void Dispose()
    {
        _stoppingCts.Cancel();
    }
    #endregion
}
