using System.ComponentModel.DataAnnotations;

namespace AccessBot.Application.Configuration;

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";

    [Required] public string BotToken { get; init; } = default!;

    [Required] public string HostAddress { get; init; } = default!;
    [Required] public string Route { get; init; } = default!;
    [Required] public string SecretToken { get; init; } = default!;
}