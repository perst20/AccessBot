using NodaTime;

namespace AccessBot.Application.Models;

public class User
{
    private User()
    {
    }

    public User(long id, string? username = default, User? inviter = default)
    {
        Id = id;
        Inviter = inviter;
        Username = username;
    }

    public long Id { get; }
    
    public string? Username { get; set; }

    public Instant PaidUntil { get; set; } = Instant.MinValue;

    public int Balance { get; set; } = 0;

    public IReadOnlyCollection<User> Referrals { get; } = new HashSet<User>();

    public User? Inviter { get; }
}