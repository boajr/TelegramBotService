using Microsoft.AspNetCore.Hosting;
using Test;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddTelegramBot<BotHandler>(options =>
        {
            options.BotToken = hostContext.Configuration.GetValue<string>("Telegram:BotToken");
        });
    })
    .Build();

host.Run();
