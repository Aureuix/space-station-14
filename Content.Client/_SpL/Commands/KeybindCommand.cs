using System.Text;
using Robust.Client.Input;
using Robust.Shared.Console;
using Content.Shared.Administration;
using static Robust.Client.Input.Keyboard;
using System.Linq;

namespace Content.Client.Commands
{

    [AnyCommand]
    internal sealed partial class KeybindCommand : LocalizedCommands
    {
        [Dependency] private readonly IInputManager _inputManager = default!;


        // Because the regular bind command and its lack of properly overwriting existing binds is Robust, we have to have our own version of it. Yay.
        public override string Command => "keybind";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length is < 2 or > 5)
            {
                shell.WriteError(Loc.GetString("cmd-invalid-arg-number-error"));
                return;
            }

            var keyName = args[0].ToLower();

            if (!Enum.TryParse<Key>(keyName, true, out var keyId))
            {
                shell.WriteLine($"Key '{keyName}' is unrecognized. Binding not assigned!");
                return;
            }

            if (keyName is "control" or "alt" or "shift")
            {
                shell.WriteLine(Loc.GetString("cmd-keybind-error-modifier"));
                return;
            }

            if (args[1].IsWhiteSpace())
            {
                shell.WriteError(Loc.GetString("cmd-keybind-error-whitespace"));
                return;
            }

            var modsSort = new List<Key>();

            if (args.Any(k => k.ToLower() == "alt"))
                modsSort.Add(Key.Alt);
            if (args.Any(k => k.ToLower() == "shift"))
                modsSort.Add(Key.Shift);
            if (args.Any(k => k.ToLower() == "control"))
                modsSort.Add(Key.Control);

            Key[] mods = new Key[3];

            modsSort.CopyTo(mods);

            var registration = new KeyBindingRegistration
            {
                Function = args[1],
                BaseKey = keyId,
                Mod1 = mods[0],
                Mod2 = mods[1],
                Mod3 = mods[2],
                Type = KeyBindingType.Command
            };

            List<IKeyBinding> toRemove = [];

            foreach (IKeyBinding binding in _inputManager.AllBindings)
            {
                if (binding.BaseKey == keyId
                    && binding.Mod1 == mods[0]
                    && binding.Mod2 == mods[1]
                    && binding.Mod3 == mods[2])
                {
                    toRemove.Add(binding);
                }
            }

            foreach (IKeyBinding binding in toRemove)
            {
                _inputManager.RemoveBinding(binding);
            }

            _inputManager.RegisterBinding(registration);

            _inputManager.SaveToUserData();

            var modifiers = "";
            if (modsSort.Count > 0)
            {
                modifiers = modsSort.Count switch
                {
                    1 => $"{modsSort[0]} + ",
                    2 => $"{modsSort[0]} + {modsSort[1]} + ",
                    3 => $"{modsSort[0]} + {modsSort[1]} + {modsSort[2]} + ",
                };
            }
            shell.WriteLine($"Bound \"{args[1]}\" to {modifiers}{keyId} and saved it to config!");
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var options = Enum.GetNames<Key>().Where(k => k.ToLower() is not "control" or "alt" or "shift");
                return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-key"));
            }

            if (args.Length == 2)
            {
                return CompletionResult.FromHint(Loc.GetString("cmd-keybind-arg-command"));
            }

            if (args.Length == 3)
            {
                string[] options = { Key.Control.ToString(), Key.Alt.ToString(), Key.Shift.ToString() };
                return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-mod1"));
            }

            if (args.Length == 4)
            {
                string[] options = { };
                if (args[2] == Key.Control.ToString())
                    options = [Key.Alt.ToString(), Key.Shift.ToString()];

                if (args[2] == Key.Alt.ToString())
                    options = [Key.Control.ToString(), Key.Shift.ToString()];

                else
                    options = [Key.Alt.ToString(), Key.Control.ToString()];

                return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-mod2"));
            }

            if (args.Length == 5)
            {
                string[] options = { };
                if (!args.Contains(Key.Control.ToString()))
                    options = [Key.Control.ToString()];

                if (!args.Contains(Key.Alt.ToString()))
                    options = [Key.Alt.ToString()];
                else
                    options = [Key.Shift.ToString()];
                return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-mod3"));
            }

            return CompletionResult.Empty;
        }
    }
}

[AnyCommand]
internal sealed partial class UnbindCommand : LocalizedCommands
{
    [Dependency] private readonly IInputManager _inputManager = default!;


    // Because the regular bind command and its lack of properly overwriting existing binds is Robust, we have to have our own version of it. Yay.
    public override string Command => "unbind";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 1 or > 4)
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

        if (keyName is "control" or "alt" or "shift")
        {
            shell.WriteLine(Loc.GetString("cmd-keybind-error-modifier"));
            return;
        }

        var modsSort = new List<Key>();

        if (args.Any(k => k.ToLower() == "alt"))
            modsSort.Add(Key.Alt);
        if (args.Any(k => k.ToLower() == "shift"))
            modsSort.Add(Key.Shift);
        if (args.Any(k => k.ToLower() == "control"))
            modsSort.Add(Key.Control);

        Key[] mods = new Key[3];

        modsSort.CopyTo(mods);

        List<IKeyBinding> toRemove = [];

        foreach (IKeyBinding binding in _inputManager.AllBindings)
        {
            if (binding.BaseKey == keyId
                && binding.Mod1 == mods[0]
                && binding.Mod2 == mods[1]
                && binding.Mod3 == mods[2])
            {
                toRemove.Add(binding);
            }
        }

        foreach (IKeyBinding binding in toRemove)
        {
            _inputManager.RemoveBinding(binding);
        }

        _inputManager.SaveToUserData();

        var modifiers = "";
        if (modsSort.Count > 0)
        {
            modifiers = modsSort.Count switch
            {
                1 => $"{modsSort[0]} + ",
                2 => $"{modsSort[0]} + {modsSort[1]} + ",
                3 => $"{modsSort[0]} + {modsSort[1]} + {modsSort[2]} + ",
            };
        }

        StringBuilder removed = new StringBuilder();

        foreach (IKeyBinding binding in toRemove)
        {
            _inputManager.RemoveBinding(binding);
            removed.Append($"{binding.Function.FunctionName}\n");
        }

        if (toRemove.Count > 0)
        {
            shell.WriteLine($"Unbound the following from {modifiers}{keyId} and to config:\n{removed}");
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
            var options = Enum.GetNames<Key>().Where(k => k.ToLower() is not "control" or "alt" or "shift");
            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-key"));
        }

        if (args.Length == 2)
        {
            string[] options = { Key.Control.ToString(), Key.Alt.ToString(), Key.Shift.ToString() };
            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-mod1"));
        }

        if (args.Length == 3)
        {
            string[] options = { };
            if (args[2] == Key.Control.ToString())
                options = [Key.Alt.ToString(), Key.Shift.ToString()];

            if (args[2] == Key.Alt.ToString())
                options = [Key.Control.ToString(), Key.Shift.ToString()];

            else
                options = [Key.Alt.ToString(), Key.Control.ToString()];

            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-mod2"));
        }

        if (args.Length == 4)
        {
            string[] options = { };
            if (!args.Contains(Key.Control.ToString()))
                options = [Key.Control.ToString()];

            if (!args.Contains(Key.Alt.ToString()))
                options = [Key.Alt.ToString()];
            else
                options = [Key.Shift.ToString()];
            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-keybind-arg-mod3"));
        }

        return CompletionResult.Empty;
    }
}
