using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Shared.Console;
using Robust.Shared.Toolshed.Commands.Values;
using static Robust.Client.Input.Keyboard;

namespace Content.Client.Commands
{

    [UsedImplicitly]
    internal sealed partial class KeybindCommand : LocalizedCommands
    {
        [Dependency] private readonly IInputManager _inputManager = default!;


        // Because the regular bind command and its lack of properly overwriting existing binds is Robust, we have to have our own version of it. Yay.
        public override string Command => "keybind";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length is < 2 or > 3)
            {
                shell.WriteError(Loc.GetString("cmd-invalid-arg-number-error"));
                return;
            }

            var keyName = args[0];

            if (!Enum.TryParse<Key>(keyName, true, out var keyId))
            {
                shell.WriteLine($"Key '{keyName}' is unrecognized. Binding not assigned!");
                return;
            }

            if (args[1].IsWhiteSpace())
            {
                shell.WriteError(Loc.GetString("cmd-keybind-error-whitespace"));
                return;
            }

            var writeFile = true;

            if (args.Length == 3)
            {
                if (bool.TryParse(args[2], out bool opt))
                {
                    writeFile = opt;
                }
                else
                {
                    shell.WriteError($"Argument '{args[2]}' isn't a bool. Binding not assigned, just in case!");
                    return;
                }
            }

            var registration = new KeyBindingRegistration
            {
                Function = args[1],
                BaseKey = keyId,
                Type = KeyBindingType.Command
            };

            List<IKeyBinding> toRemove = [];
            foreach (IKeyBinding binding in _inputManager.AllBindings)
            {
                if (binding.BaseKey == keyId)
                {
                    toRemove.Add(binding);
                }
            }

            foreach (IKeyBinding binding in toRemove)
            {
                _inputManager.RemoveBinding(binding);
            }

            _inputManager.RegisterBinding(registration);

            if (writeFile) _inputManager.SaveToUserData();

            shell.WriteLine($"Bound \"{args[1]}\" to key {keyId} and {(writeFile ? "saved" : "did not save")} it to config!");
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var options = Enum.GetNames<Key>();
                return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-key"));
            }

            if (args.Length == 2)
            {
                return CompletionResult.FromHint(Loc.GetString("cmd-keybind-arg-command"));
            }

            if (args.Length == 3)
            {
                return CompletionResult.FromHintOptions(["false"], Loc.GetString("cmd-keybind-save-mode"));
            }

            return CompletionResult.Empty;
        }
    }
}

[UsedImplicitly]
internal sealed partial class UnbindCommand : LocalizedCommands
{
    [Dependency] private readonly IInputManager _inputManager = default!;


    // Because the regular bind command and its lack of properly overwriting existing binds is Robust, we have to have our own version of it. Yay.
    public override string Command => "unbind";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 1 or > 2)
        {
            shell.WriteError(Loc.GetString("cmd-invalid-arg-number-error"));
            return;
        }

        var keyName = args[0];

        if (!Enum.TryParse<Key>(keyName, true, out var keyId))
        {
            shell.WriteLine($"Key '{keyName}' is unrecognized. Bindings not removed!");
            return;
        }

        var writeFile = true;

        if (args.Length == 2)
        {
            if (bool.TryParse(args[1], out bool opt))
            {
                writeFile = opt;
            }
            else
            {
                shell.WriteError($"Argument '{args[1]}' isn't a bool. Bindings not removed, just in case!");
                return;
            }
        }
        List<IKeyBinding> toRemove = [];
        foreach (IKeyBinding binding in _inputManager.AllBindings)
        {
            if (binding.BaseKey == keyId)
            {
                toRemove.Add(binding);
            }
        }

        StringBuilder removed = new StringBuilder();

        foreach (IKeyBinding binding in toRemove)
        {
            _inputManager.RemoveBinding(binding);
            removed.Append($"{binding.Function.FunctionName}\n");
        }

        if (toRemove.Count > 0)
        {
            if (writeFile)
            {
                _inputManager.SaveToUserData();
            }
            shell.WriteLine($"Unbound the following from key {keyId} and {(writeFile ? "saved" : "did not save")} to config:\n{removed}");
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-unbind-info-empty"));
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = Enum.GetNames<Key>();
            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-key"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(["false"], Loc.GetString("cmd-keybind-save-mode"));
        }

        return CompletionResult.Empty;
    }
}
