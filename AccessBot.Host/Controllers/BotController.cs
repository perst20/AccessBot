using AccessBot.Application.Services;
using AccessBot.Filters;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace AccessBot.Controllers;

[ApiController]
[Route("/")]
public class BotController : ControllerBase
{
    private readonly UpdateHandlers _handlers;

    public BotController(UpdateHandlers handlers)
    {
        _handlers = handlers;
    }

    [HttpPost]
    [Route("/")]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        CancellationToken cancellationToken)
    {
        await _handlers.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}