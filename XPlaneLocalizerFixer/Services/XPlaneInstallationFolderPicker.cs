using NativeFileDialogs.Net;

namespace XPlaneLocalizerFixer.Services;

public sealed class XPlaneInstallationFolderPicker : IXPlaneInstallationFolderPicker
{
    public string? SelectFolder()
    {
        var status = Nfd.PickFolder(out var selectedPath);

        return status switch
        {
            NfdStatus.Ok => selectedPath
                            ?? throw new InvalidOperationException(
                                "The folder picker succeeded without returning a directory."),

            NfdStatus.Cancelled => null,

            _ => throw new InvalidOperationException(
                $"Unexpected folder picker status: {status}")
        };
    }
}