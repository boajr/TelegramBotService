using Boa.TelegramBotService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
