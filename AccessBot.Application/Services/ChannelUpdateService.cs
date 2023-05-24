using AccessBot.Application.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NodaTime;
using TL;
using WTelegram;

namespace AccessBot.Application.Services;

public class ChannelUpdateService : BackgroundService
{
    private readonly IOptions<ClientConfiguration> _configuration;
    private readonly ICodeStorage _code;
    private readonly IServiceProvider _serviceProvider;
    private readonly InvitationLink _link;
    private Client _client = default!;

    public ChannelUpdateService(IOptions<ClientConfiguration> configuration, ICodeStorage code,
        IServiceProvider serviceProvider, InvitationLink link)
    {
        _configuration = configuration;
        _code = code;
        _serviceProvider = serviceProvider;
        _link = link;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new Client(_configuration.Value.ApiId, _configuration.Value.ApiHash);
        var result = await _client.Login(_configuration.Value.Phone);
        if (result == "verification_code")
            result = await _client.Login((await _code.Get()).ToString());
        if (result is not null)
            throw new Exception("Login failed");

        var channels = (await _client.Messages_GetAllChats()).chats
            .Where(x => x.Value.IsChannel)
            .ToList();

        if (channels.Count != 1)
            throw new Exception("There must be only one channel on account");

        var channelInputPeer = channels.First().Value;
        var peer = channelInputPeer.ToInputPeer() as InputPeerChannel;

        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync(new InputPeerChannel(peer.channel_id, peer.access_hash), stoppingToken);
            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task DoWorkAsync(InputPeerChannel inputPeer, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = await db.Users.ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.Username))
                continue;
            if (user.PaidUntil < SystemClock.Instance.GetCurrentInstant())
            {
                var bannedRights = new ChatBannedRights
                {
                    flags = ChatBannedRights.Flags.view_messages
                };
                var userPeer = await _client.Contacts_ResolveUsername(user.Username);
                await _client.Channels_EditBanned(inputPeer, userPeer, bannedRights);
            }
            else
            {
                var bannedRights = new ChatBannedRights
                {
                    flags = 0
                };
                var userPeer = await _client.Contacts_ResolveUsername(user.Username);
                await _client.Channels_EditBanned(inputPeer, userPeer, bannedRights);
            }
        }
        
        _link.Link =
            ((ChatInviteExported) await _client.Messages_ExportChatInvite(inputPeer,
                expire_date: DateTime.Now + TimeSpan.FromHours(1))).link;
    }
}