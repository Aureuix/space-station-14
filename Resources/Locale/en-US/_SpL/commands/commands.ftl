cmd-noise-desc = Makes an emote sound without saying it in the chat.
cmd-noise-help = Usage: {$command} <emote with sound>
cmd-noise-error = Buh?? How???
cmd-noise-hints = Noises to make!

cmd-keybind-desc = Binds a key to a console command, and saves it by default. This will overwrite existing assignments!
cmd-keybind-arg-key = <Key>
cmd-keybind-arg-mod1 = <Modifier 1>
cmd-keybind-arg-mod2 = <Modifier 2>
cmd-keybind-arg-mod3 = <Modifier 3>
cmd-keybind-arg-command = "Console Command" e.g. "noise Wurble"
cmd-keybind-save-mode = Save to config? True by default, set to false to clear on reload as long as you don't save elsewhere.
cmd-keybind-error-whitespace = Command can't be blank. Are you trying to remove bindings from the key? Try the unbind command!
cmd-keybind-error-modifier = This argument can't be a modifier key! Binding not assigned.
cmd-keybind-help = Usage: keybind <Key> <"Console Command"> <Optional, Modifier 1> <Optional, Modifier 2> <Optional, Modifier 3>
    Example: keybind f13 "noise Wurble" control shift

    Unlike the Robust Toolbox bind command, this can be used more than once without relaunching. 
    Try to avoid engine-bound keys here, or you may experience some issues.
    _

cmd-unbind-desc = Remove all bindings from a key, and saves it by default. 
cmd-unbind-info-empty = There was nothing to unbind on this key!
cmd-unbind-help = Usage: unbind <Key> <Optional, Modifier 1> <Optional, Modifier 2> <Optional, Modifier 3>
    Example: unbind f13

    Clears all assignments for the specified key. Optionally applies without saving. Saving elsewhere will still include this, so be careful!
    _