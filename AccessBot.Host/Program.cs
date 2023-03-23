using AccessBot.Application.Configuration;
using AccessBot.Application.DI;
using AccessBot.Persistence.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var botConfig = sp.GetService<IOptions<BotConfiguration>>()?.Value ??
                        throw new NullReferenceException("No bot configuration");
        var options = new TelegramBotClientOptions(botConfig.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services
    .AddApplication()
    .AddPersistence(options =>
    {
        options.UseNpgsql("Host=localhost;Database=User;User ID=postgres;Password=postgres",
            o =>
            {
                o.UseNodaTime();
                o.MigrationsAssembly("AccessBot.Migrations");
            });
    });

builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();
app.MapControllers();
app.Run();