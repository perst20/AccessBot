using AccessBot.Application.Configuration;
using AccessBot.Application.DI;
using AccessBot.Application.Services;
using AccessBot.Persistence.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var botConfig = sp.GetService<IOptions<BotConfiguration>>()?.Value ??
                        throw new NullReferenceException("No bot configuration");
        var options = new TelegramBotClientOptions(botConfig.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services
    .AddApplication(
        x => builder.Configuration.GetSection(nameof(BotConfiguration)).Bind(x),
        x => builder.Configuration.GetSection(nameof(ClientConfiguration)).Bind(x))
    .AddPersistence(options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("postgres") ??
            "Host=localhost;Database=User;User ID=postgres;Password=postgres",
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
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

// Here is the migration executed
    dbContext.Database.Migrate();
}

app.Run();