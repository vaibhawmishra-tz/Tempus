using Microsoft.AspNetCore.Mvc;

namespace Tempus.Sample.WebApi.Controllers;

[ApiController]
[Route("json-demo")]
internal sealed class JsonDemoController : ControllerBase
{
    /// <summary>
    /// Returns a demo DTO showing all three Tempus JSON converter attributes.
    /// Pass X-Timezone: America/New_York (or ?tz=) to shift the [UserLocalTime] field.
    /// </summary>
    [HttpGet]
    public IActionResult Get() => Ok(new EventDto
    {
        Title     = "Tempus demo event",
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
        StartsAt  = DateTimeOffset.UtcNow.AddHours(3),
        UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-15),
    });
}
