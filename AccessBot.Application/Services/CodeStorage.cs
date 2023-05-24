namespace AccessBot.Application.Services;

public interface ICodeStorage
{
    bool TrySet(int value);

    public Task<int> Get();
}

internal sealed class CodeStorage : ICodeStorage
{
    private readonly TaskCompletionSource<int> _code = new ();

    public bool TrySet(int value) => _code.TrySetResult(value);

    public Task<int> Get() => _code.Task;
}