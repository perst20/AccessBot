using Microsoft.EntityFrameworkCore;
using NodaTime;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = Telegram.Bot.Types.BotCommand;
using Message = Telegram.Bot.Types.Message;
using Update = Telegram.Bot.Types.Update;
using User = AccessBot.Application.Models.User;

namespace AccessBot.Application.Services;

public class UpdateHandlers
{
    private readonly ITelegramBotClient _botClient;
    private readonly IChatStates _chatStates;
    private readonly AppDbContext _context;
    private readonly IClock _clock;
    private readonly InvitationLink _link;

    public UpdateHandlers(ITelegramBotClient botClient, IChatStates chatStates, AppDbContext context, IClock clock,
        InvitationLink link)
    {
        _botClient = botClient;
        _chatStates = chatStates;
        _context = context;
        _clock = clock;
        _link = link;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken token)
    {
        switch (update)
        {
            case {Message: { } message}:
                await HandleOnMessage(message, token);
                break;
        }
    }

    private async Task HandleOnMessage(Message message, CancellationToken token)
    {
        var chatId = message.Chat.Id;
        var username = message.Chat.Username;
        var state = _chatStates.GetOrAdd(chatId);
        switch (message)
        {
            case {Text: "/start"}:
                await Init(chatId, token);
                if (state == ChatState.New)
                    await NewState(state, chatId, token);
                else
                    await NewState(ChatState.Start, chatId, token);
                break;
            case {Text: "/pay"}:
                await NewState(ChatState.Pay, chatId, token);
                break;
            case {Text: { } text} when int.TryParse(text, out var number):
                switch (state)
                {
                    case ChatState.Pay:
                        await AddBalance(chatId, username, number, token);
                        await NewState(ChatState.Start, chatId, token);
                        break;
                    case ChatState.Prolong:
                        await Prolong(chatId, username, number, token);
                        await NewState(ChatState.Start, chatId, token);
                        break;
                }

                break;
            case {Text: "/balance"}:
                await GetBalance(chatId, username, token);
                await NewState(ChatState.Start, chatId, token);
                break;
            case {Text: "/prolong"}:
                await NewState(ChatState.Prolong, chatId, token);
                break;
            case {Text: "/state"}:
                await GetSubscriptionState(chatId, username, token);
                await NewState(ChatState.Start, chatId, token);
                break;
            default:
                await IncorrectInput(chatId, token);
                break;
        }
    }

    private async Task Prolong(long chatId, string? username, int days, CancellationToken token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == chatId, token);
        if (user is null)
        {
            user = new User(chatId);
            await _context.Users.AddAsync(user, token);
        }

        user.Username = username;

        if (days is < -1000 or > 1000)
        {
            await _botClient.SendTextMessageAsync(chatId,
                $"Не ломай, умник", cancellationToken: token);
            return;
        }

        if (user.Balance < days * 10)
        {
            await _botClient.SendTextMessageAsync(chatId, "Недостаточно средств!", cancellationToken: token);
        }
        else
        {
            user.Balance -= days * 10;
            user.PaidUntil = Instant.Max(user.PaidUntil, _clock.GetCurrentInstant()) + Duration.FromDays(days);
            await _botClient.SendTextMessageAsync(chatId,
                $"Ваша подписка продлена до {user.PaidUntil.ToString("dd.MM.yyyy", null)}. Вот ссылка на вступление в канал: {_link.Link}",
                cancellationToken: token);
        }

        await _context.SaveChangesAsync(token);
    }

    private async Task AddBalance(long chatId, string? username, int balance, CancellationToken token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == chatId, token);
        if (user is null)
        {
            user = new User(chatId);
            await _context.Users.AddAsync(user, token);
        }

        user.Username = username;

        if (balance is < 0)
        {
            await _botClient.SendTextMessageAsync(chatId,
                "Не ломай, умник", cancellationToken: token);
            return;
        }

        user.Balance += balance;
        if (user.Balance < 0)
        {
            user.Balance = 0;
        }

        await _context.SaveChangesAsync(token);

        await _botClient.SendTextMessageAsync(chatId,
            $"Баланс пополнен!", cancellationToken: token);
    }

    private async Task GetBalance(long chatId, string? username, CancellationToken token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == chatId, token);
        if (user is null)
        {
            user = new User(chatId);
            await _context.Users.AddAsync(user, token);
        }

        user.Username = username;

        await _context.SaveChangesAsync(token);

        await _botClient.SendTextMessageAsync(chatId, $"Текущий баланс: {user.Balance}", cancellationToken: token);
    }

    private async Task GetSubscriptionState(long chatId, string? username, CancellationToken token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == chatId, token);
        if (user is null)
        {
            user = new User(chatId);
            await _context.Users.AddAsync(user, token);
        }

        user.Username = username;

        await _context.SaveChangesAsync(token);

        if (user.PaidUntil < SystemClock.Instance.GetCurrentInstant())
        {
            await _botClient.SendTextMessageAsync(chatId, "Подписка неактивна. Пополните баланс",
                cancellationToken: token);
        }
        else
        {
            await _botClient.SendTextMessageAsync(chatId,
                $"Ваша подписка активна до {user.PaidUntil.ToString("dd.MM.yyyy", null)}. Вот ссылка на вступление в канал: {_link.Link}",
                cancellationToken: token);
        }
    }

    private async Task IncorrectInput(long chatId, CancellationToken token)
    {
        await _botClient.SendTextMessageAsync(chatId, "Некорректный ввод.", cancellationToken: token);
    }

    private async Task NewState(ChatState newState, long chatId, CancellationToken token)
    {
        _chatStates.Set(chatId, newState);
        switch (newState)
        {
            case ChatState.New:
                await _botClient.SendTextMessageAsync(chatId,
                    "Приветствую тебя, дорогой пользователь!" +
                    "Я PayMasterBot и я предоставляю доступ к платному контенту канала. " +
                    "Я готов помочь тебе - просто выбери нужное действие из меню ниже:",
                    cancellationToken: token);
                _chatStates.Set(chatId, ChatState.Start);
                break;
            case ChatState.Start:
                await _botClient.SendTextMessageAsync(chatId,
                    "Выбери действие в меню:",
                    cancellationToken: token);
                break;
            case ChatState.Pay:
                await _botClient.SendTextMessageAsync(chatId,
                    "Введи сумму пополнения:",
                    cancellationToken: token);
                break;
            case ChatState.Prolong:
                await _botClient.SendTextMessageAsync(chatId,
                    "Введи количество дней подписки:",
                    cancellationToken: token);
                break;
        }
    }

    private async Task Init(long chatId, CancellationToken token)
    {
        await _botClient.SetMyCommandsAsync(new BotCommand[]
        {
            new()
            {
                Command = "/start",
                Description = "Начать сначала"
            },
            new()
            {
                Command = "/balance",
                Description = "Мой баланс"
            },
            new()
            {
                Command = "/pay",
                Description = "Пополнить баланс"
            },
            new()
            {
                Command = "/prolong",
                Description = "Продлить подписку"
            },
            new()
            {
                Command = "/state",
                Description = "Текущее состояние подписки"
            }
        }, cancellationToken: token);
        await _botClient.SetChatMenuButtonAsync(chatId, new MenuButtonCommands(), cancellationToken: token);
    }
}