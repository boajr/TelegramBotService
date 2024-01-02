using Boa.TelegramBotService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Hosting;

public static class TelegramBotExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, Action<TelegramBotOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services);

        // crea la configurazione
        services.Configure(setupAction);

        // aggiunge il servizio
        services.TryAddSingleton<TelegramBotService>();
        return services.AddHostedService(provider => provider.GetRequiredService<TelegramBotService>());
    }
}
