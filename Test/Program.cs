using Boa.TelegramBotService;
using Microsoft.AspNetCore.Hosting;
using Test;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddTelegramBot(options =>
        {
            options.BotToken = hostContext.Configuration.GetValue<string>("Telegram:BotToken")
            ?? throw new Exception("BotToken value must be specified.");
        });

        services.AddSingleton<ITelegramBotHandler, BotHandlerUsage>();
        services.AddSingleton<ITelegramBotHandler, BotHandler>();
    })
    .Build();

host.Run();
