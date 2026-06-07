namespace Tempus.AspNetCore.Context;

public interface ITempusUserContextFactory
{
    ITempusUserContext Create(string ianaTimeZoneId);
}
