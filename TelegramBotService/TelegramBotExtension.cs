using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telegram;
using Telegram.Bot.Polling;

namespace Microsoft.AspNetCore.Hosting
{
    public static class TelegramBotExtensions
    {
        public static IServiceCollection AddTelegramBot<THandler>(this IServiceCollection services, Action<TelegramBotOptions<THandler>> setupAction)
            where THandler : IUpdateHandler
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // crea la configurazione
            services.Configure(setupAction);

            // aggiunge l'handler
            services.TryAddSingleton(typeof(THandler));

            // aggiunge il servizio
            services.TryAddSingleton<TelegramBotService<THandler>>();
            return services.AddHostedService(provider => provider.GetService<TelegramBotService<THandler>>());
        }
    }
}
