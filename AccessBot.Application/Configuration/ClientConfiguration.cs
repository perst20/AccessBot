using System.ComponentModel.DataAnnotations;

namespace AccessBot.Application.Configuration;

public class ClientConfiguration
{
    [Required] public string Phone { get; init; } = default!;
    [Required] public int ApiId { get; init; } 
    [Required] public string ApiHash { get; init; } = default!;
}