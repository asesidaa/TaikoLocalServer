using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Tests;

/// <summary>The access rules for tests: Local mode (no sign-in, every check passes) unless a test asks for Server mode.</summary>
public sealed class TestServerAccess(ServerMode mode = ServerMode.Local, AccessRules? rules = null) : IServerAccess
{
    public ServerMode Mode => mode;
    public AccessRules Rules { get; } = rules ?? new AccessRules();
    public ServerStatus Status { get; } = new();
    public byte[] SigningKey { get; } = new byte[32];
    public bool SetupPending => false;

    public Task UpdateRulesAsync(AccessRules rules, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task UpdateStatusAsync(ServerStatus status, CancellationToken cancellationToken) => Task.CompletedTask;
    public bool TryUseSetupCode(string code) => false;
    public void LogSetupCode() { }
}

/// <summary>An avatar renderer that's off and renders nothing.</summary>
public sealed class NullAvatarRenderer : IAvatarRenderer
{
    public AvatarRendererStatus Status { get; } = new(false, null, false, 0, 0, 0, null, null);
    public void Enqueue(uint baid, bool changed = false) { }
    public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task SetCards3DAsync(bool on, CancellationToken cancellationToken) => Task.CompletedTask;
    public bool IsRendererToken(string token) => false;
}

/// <summary>No cabinets known, none restricted.</summary>
public sealed class NullCabinetRegistry : ICabinetRegistry
{
    public bool Restricted => false;
    public Task InitializeAsync() => Task.CompletedTask;
    public bool Seen(string chassisId, string shopId, string version) => true;
    public IReadOnlyList<Cabinet> List() => [];
    public Cabinet? Find(string chassisId) => null;
    public Task UpdateAsync(string chassisId, Action<Cabinet> change, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<bool> RemoveAsync(string chassisId, CancellationToken cancellationToken) => Task.FromResult(false);
}
