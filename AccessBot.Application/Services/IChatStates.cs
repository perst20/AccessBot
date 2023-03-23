using System.Collections.Concurrent;

namespace AccessBot.Application.Services;

public enum ChatState
{
    New,
    Start,
    Pay,
    Prolong
}

public interface IChatStates
{
    ChatState GetOrAdd(long chatId, ChatState defaultState = default);
    ChatState Set(long chatId, ChatState state);
}

internal sealed class ChatStates : IChatStates
{
    private readonly ConcurrentDictionary<long, ChatState> _chatStates = new();

    public ChatState GetOrAdd(long chatId, ChatState defaultState) =>
        _chatStates.GetOrAdd(chatId, defaultState);

    public ChatState Set(long chatId, ChatState state) => _chatStates[chatId] = state;

}