using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
}
