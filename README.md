# TelegramBotService

A simple AspNetCore service that runs a Telegram bot.

## Description

The following contains a description of each sub-directory.

* `TelegramBotService`: Contains extensions to [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) to run it as a service.
* `Test`: Contains a worker used for functional testing.

## Getting Started

To add the service, simply call the `AddTelegramBot` services extension method passing the Telegram bot token among the options

```csharp
services.AddTelegramBot(options =>
{
    options.BotToken = hostContext.Configuration.GetValue<string>("Telegram:BotToken")
        ?? throw new Exception("BotToken value must be specified.");
});
```

subsequently you can add one or more `ITelegramBotHandlers` to manage the bot requests

```csharp
services.AddSingleton<ITelegramBotHandler, BotHandler1>();
services.AddScoped<ITelegramBotHandler, BotHandler2>();
```

each individual request will be forwarded to handlers, based on the `Order` number, until one handles it.

Handlers can be added at runtime by calling the `AddUpdateHandler` service method and removed by calling the `RemoveUpdateHandler` method.