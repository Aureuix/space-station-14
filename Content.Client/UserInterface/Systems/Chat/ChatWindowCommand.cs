using System.IO;
using Content.Shared.Chat;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Client.UserInterface.Systems.Chat;

/// <summary>
/// Command which creates a window containing a chatbox
/// </summary>
[UsedImplicitly]
public sealed class ChatWindowCommand : LocalizedCommands
{
    public override string Command => "chatwindow";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var window = new ChatWindow();
        window.OpenCentered();
    }
}

/// <summary>
/// Command which creates a window containing a chatbox configured for admin use
/// </summary>
[UsedImplicitly]
public sealed class AdminChatWindowCommand : LocalizedCommands
{
    public override string Command => "achatwindow";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var window = new ChatWindow();
        window.ConfigureForAdminChat();
        window.OpenCentered();
    }
}

/// <summary>
/// Command which exports the local chat history, for importing into a (later-to-be-revamped) chat window
/// </summary>
[UsedImplicitly]
public sealed class ExportChatCommand : LocalizedCommands
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IFileDialogManager _dialogManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private bool _currentlyExportingLogs = false;
    private ISawmill _sawmill = default!;
    public override string Command => "savechatlog";
    public override string Description => "Opens a dialog to save your chat history as text.";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_currentlyExportingLogs)
            return;

        _currentlyExportingLogs = true;

        var file = await _dialogManager.SaveFile(new FileDialogFilters(new FileDialogFilters.Group("log")));

        if (file == null)
            return;

        _sawmill = _logManager.GetSawmill("SpL");

        try
        {
            ChatUIController _controller = _userInterfaceManager.GetUIController<ChatUIController>();
            await using var writer = new StreamWriter(file.Value.fileStream, bufferSize: 4096);
            foreach ((var time, ChatMessage msg) in _controller.History)
            {
                string message = msg.WrappedMessage.Replace("\n", "\\n").Replace("\r", "\\r");
                await writer.WriteLineAsync($"{time.Value}:{msg.Channel}\n{message}");
            }
        }
        catch (Exception exc)
        {
            _sawmill.Error($"Error when exporting chat log:\n{exc.StackTrace}");
        }
        finally
        {
            await file.Value.fileStream.DisposeAsync();
            _currentlyExportingLogs = false;
            _sawmill.Info($"Successfully saved all chat history. Log viewer to use these files is coming soon!");
        }
    }
}
