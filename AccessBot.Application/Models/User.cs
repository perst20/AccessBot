using NodaTime;

namespace AccessBot.Application.Models;

public class User
{
    private User()
    {
    }

    public User(long id, User? inviter = default)
    {
        Id = id;
        Inviter = inviter;
    }

    public long Id { get; }

    public Instant PaidUntil { get; set; } = Instant.MinValue;

    public int Balance { get; set; } = 0;

    public IReadOnlyCollection<User> Referrals { get; } = new HashSet<User>();

    public User? Inviter { get; }
}