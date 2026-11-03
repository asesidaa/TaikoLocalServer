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
    public string? SetupCode => null;
    public bool IsSetupCode(string? code) => false;
    public Task FinishSetupAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task UpdateRulesAsync(AccessRules rules, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task UpdateStatusAsync(ServerStatus status, CancellationToken cancellationToken) => Task.CompletedTask;
    public bool TryUseSetupCode(string code) => false;
    public void LogSetupCode() { }
}

/// <summary>No cabinets known, none restricted.</summary>
public sealed class NullCabinetRegistry : ICabinetRegistry
{
    public bool Restricted => false;
    public Task InitializeAsync() => Task.CompletedTask;
    public bool Seen(string chassisId, string shopId, string version) => true;
    public void Located(string chassisId, string locationName) { }
    public IReadOnlyList<Cabinet> List() => [];
    public Cabinet? Find(string chassisId) => null;
    public Task UpdateAsync(string chassisId, Action<Cabinet> change, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<bool> RemoveAsync(string chassisId, CancellationToken cancellationToken) => Task.FromResult(false);
}
