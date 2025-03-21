using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Boa.TelegramBotService;

public sealed class TelegramBotService(IOptions<TelegramBotOptions> options,
                                       IServiceProvider serviceProvider,
                                       ILogger<TelegramBotService> logger) : BackgroundService
{
    private readonly List<ITelegramBotHandler> _handlers = [];
    private readonly IOptions<TelegramBotOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<TelegramBotService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public TelegramBotClient? BotClient { get; private set; }

    public void AddUpdateHandler(ITelegramBotHandler handler)
    {
        int idx = _handlers.Count;
        int s = 0;
        int e = idx - 1;
        int order = handler.Order;

        while (e >= s)
        {
            idx = (s + e) / 2;
            if (_handlers[idx].Order <= order)
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

    #region BackgroundService
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // crea il TelegramBotClient
            BotClient = new(new TelegramBotClientOptions(_options.Value.BotToken, _options.Value.BotBaseUrl, _options.Value.BotUseTestEnvironment)
            {
                RetryThreshold = _options.Value.BotRetryThreshold,
                RetryCount = _options.Value.BotRetryCount
            }, _options.Value.BotHttpClient, stoppingToken);

            // prelevo le mie credenziali per stamparle nei log (ripasso lo stoppingToken anche se non servirebbe, così il VisualStudio è contento)
            try
            {
                User me = await BotClient.GetMe(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Connected as user {Username} (botId: {Id})", me.Username, me.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to get bot credentials: {Message}", ex.Message);
            }

            // creo la richiesta di update
            var request = new GetUpdatesRequest
            {
                Limit = 100,
                Offset = 0,
                AllowedUpdates = [],
            };

            // entro nel ciclo di polling delle nuove richieste
            while (!stoppingToken.IsCancellationRequested)
            {
                request.Timeout = (int)BotClient.Timeout.TotalSeconds;

                try
                {
                    // chiedo al server gli ultimi update, se non ce ne sono passo alla richiesta successiva
                    Update[] updates = await BotClient.SendRequest(request, stoppingToken).ConfigureAwait(false);
                    if (updates.Length == 0)
                    {
                        continue;
                    }

                    // creo uno scope per poterne utilizzare i servizi
                    using IServiceScope scope = _serviceProvider.CreateScope();

                    // prelevo tutti i servizi registrati per gestire le richieste di telegram
                    IEnumerable<ITelegramBotHandler> hh = scope.ServiceProvider.GetServices<ITelegramBotHandler>();

                    // creo un array che conterrà tutti i servizi ordinati
                    ITelegramBotHandler[] servHandlers = new ITelegramBotHandler[hh.Count()];
                    int servCount = 0;

                    // aggiungo all'array tutti i servizi ordinandoli
                    foreach (ITelegramBotHandler handler in hh)
                    {
                        int idx = servCount;
                        int s = 0;
                        int e = servCount - 1;
                        int order = handler.Order;

                        while (e >= s)
                        {
                            idx = (s + e) / 2;
                            if (servHandlers[idx].Order <= order)
                            {
                                s = ++idx;
                            }
                            else
                            {
                                e = idx - 1;
                            }
                        }

                        if (idx < servCount)
                        {
                            Array.Copy(servHandlers, idx, servHandlers, idx + 1, servCount - idx);
                        }
                        servHandlers[idx] = handler;
                        ++servCount;
                    }

                    // processo tutte le richieste appena lette
                    foreach (var update in updates)
                    {
                        // aggiorno il numero della prossima richiesta da chiedere al server
                        request.Offset = update.Id + 1;

                        // processo la richiesta
                        int servIdx = 0;
                        int regIdx = 0;
                        while (!stoppingToken.IsCancellationRequested && (servIdx < servCount || regIdx < _handlers.Count))
                        {
                            // cerco qual è il prossimo handler in ordine di priorità
                            ITelegramBotHandler handler;
                            if (servIdx < servCount && regIdx < _handlers.Count)
                            {
                                if (servHandlers[servIdx].Order <= _handlers[regIdx].Order)
                                {
                                    handler = servHandlers[servIdx++];
                                }
                                else
                                {
                                    handler = _handlers[regIdx++];
                                }
                            }
                            else if (servIdx < servCount)
                            {
                                handler = servHandlers[servIdx++];
                            }
                            else
                            {
                                handler = _handlers[regIdx++];
                            }

                            // chiamo l'handler, se ritorna true passo alla prossima richiesta
                            try
                            {
                                if (await handler.HandleUpdateAsync(BotClient, update, stoppingToken).ConfigureAwait(false))
                                {
                                    break;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                continue;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("{Message}", ex.Message);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError("{Message}", ex.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            _logger.LogError("{Message}", ex.Message);
        }
    }
    #endregion
}
