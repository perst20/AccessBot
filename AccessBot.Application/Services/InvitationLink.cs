namespace AccessBot.Application.Services;

public class InvitationLink
{
    private string? _link;

    public string? Link
    {
        get => _link;
        set => Interlocked.Exchange(ref _link, value);
    }
}