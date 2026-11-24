using System.IO;
using System.Text;
using Content.Shared.Administration;
using Content.Shared.Chat;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Console;
using static Content.Shared.Administration.Notes.AdminMessageEuiState;

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
[AnyCommand]
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
        {
            _currentlyExportingLogs = false;
            return;
        }

        _sawmill = _logManager.GetSawmill("SpL");

        var messages = 0;

        var export = new StringBuilder();

        try
        {
            ChatUIController _controller = _userInterfaceManager.GetUIController<ChatUIController>();
            await using var writer = new StreamWriter(file.Value.fileStream, bufferSize: 8192);
            foreach ((var time, ChatMessage msg) in _controller.History)
            {
                var body = msg.WrappedMessage.Replace("\n", "\\n").Replace("\r", "\\r");
                export.AppendLine($"{time.Value}:{msg.Channel}\n{body}\n");
                messages++;
            }
            await writer.WriteLineAsync($"{export}");
            _sawmill.Info($"Successfully saved {messages} messages of chat history. Log viewer to use these files is coming soon!");
        }
        catch (Exception exc)
        {
            /* Commented out until this can give more useful error messages
            _sawmill.Error($"Error when exporting chat log:\n{exc.StackTrace}");
            */
        }
        finally
        {
            await file.Value.fileStream.DisposeAsync();
            _currentlyExportingLogs = false;
        }
    }
}