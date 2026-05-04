namespace TaikoLocalServer.Application.Abstractions;

public interface IClock
{
    DateTime Now { get; }

    DateTime UtcNow { get; }
}
