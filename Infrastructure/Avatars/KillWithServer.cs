using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TaikoLocalServer.Infrastructure.Avatars;

/// <summary>
/// Windows: puts this server in a job object that ends everything in it when the job closes, which Windows does
/// when the server exits in any way (shut down, crashed, ended in Task Manager, its console closed). Processes the
/// server starts afterwards, such as the avatar renderer's Edge or Chrome and all of its helpers, join the job, so
/// none outlive the server.
/// </summary>
internal static class KillWithServer
{
    private static nint job;

    /// <summary>
    /// Linux, which has no job objects: ends the browsers a killed earlier run of this server left running, found by
    /// their profile folder (each server has its own). Called before the renderer starts a browser of its own.
    /// </summary>
    public static void EndOrphans(string profileDir)
    {
        if (!OperatingSystem.IsLinux()) return;
        var marker = "--user-data-dir=" + profileDir;
        foreach (var dir in Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(dir), out var pid) || pid == Environment.ProcessId) continue;
            try
            {
                if (File.ReadAllText(Path.Combine(dir, "cmdline")).Split('\0').Contains(marker)) Process.GetProcessById(pid).Kill();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
            {
                // Gone already, or another user's process.
            }
        }
    }

    public static void Enable()
    {
        if (!OperatingSystem.IsWindows() || job != 0) return;
        var handle = CreateJobObject(0, null);
        if (handle == 0) return;
        var info = new ExtendedLimits { Basic = new BasicLimits { LimitFlags = KillOnJobClose } };
        var size = Marshal.SizeOf<ExtendedLimits>();
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, ptr, false);
            if (!SetInformationJobObject(handle, JobObjectExtendedLimitInformation, ptr, (uint)size)
                || !AssignProcessToJobObject(handle, Process.GetCurrentProcess().Handle))
            {
                CloseHandle(handle);
                return;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        // Never closed: the handle lives as long as the server, and Windows closing it is what ends the job.
        job = handle;
    }

    private const uint KillOnJobClose = 0x2000;
    private const int JobObjectExtendedLimitInformation = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct BasicLimits
    {
        public long PerProcessUserTimeLimit, PerJobUserTimeLimit;
        public uint LimitFlags;
        public nuint MinimumWorkingSetSize, MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public nuint Affinity;
        public uint PriorityClass, SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExtendedLimits
    {
        public BasicLimits Basic;
        public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount, ReadTransferCount, WriteTransferCount, OtherTransferCount;
        public nuint ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateJobObject(nint attributes, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(nint job, int infoClass, nint info, uint length);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(nint job, nint process);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint handle);
}
