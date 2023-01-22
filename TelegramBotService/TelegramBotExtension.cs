using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TelegramBotService;

namespace Microsoft.AspNetCore.Hosting
{
    public static class TelegramBotExtensions
    {
        public static IServiceCollection AddTelegramBot(this IServiceCollection services, Action<TelegramBotOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // crea la configurazione
            services.Configure(setupAction);

            // aggiunge il servizio
            services.TryAddSingleton<TelegramBotService.TelegramBotService>();
            return services.AddHostedService(provider => provider.GetRequiredService<TelegramBotService.TelegramBotService>());
        }
    }
}
