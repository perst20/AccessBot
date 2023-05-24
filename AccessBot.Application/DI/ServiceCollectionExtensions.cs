using System.Collections;
using System.Net.Sockets;
using AccessBot.Application.Configuration;
using AccessBot.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using Telegram.Bot;

namespace AccessBot.Application.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        Action<BotConfiguration> botConfigure, Action<ClientConfiguration> clientConfigure)
    {
        services.Configure(botConfigure);
        services.Configure(clientConfigure);
        services.AddOptions<BotConfiguration>();
        services.AddOptions<ClientConfiguration>();
        // services.AddSingleton<IBotStateService, BotStateService>();
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
        services.AddHostedService<ConfigureWebhook>();
        services.AddSingleton<IChatStates, ChatStates>();
        services.AddScoped<UpdateHandlers>();
        services.AddTransient<IClock>(_ => SystemClock.Instance);
        services.AddSingleton<ICodeStorage, CodeStorage>();
        services.AddHostedService<ChannelUpdateService>();
        services.AddSingleton<InvitationLink>();

        return services;
    }
}