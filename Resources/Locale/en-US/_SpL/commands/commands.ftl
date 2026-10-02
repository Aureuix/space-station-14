cmd-noise-desc = Makes an emote sound without saying it in the chat.
cmd-noise-help = Usage: {$command} <emote with sound>
cmd-noise-error = Buh?? How???
cmd-noise-hints = Noises to make!

cmd-keybind-desc = Binds a key to a console command, and saves it by default. This will overwrite existing assignments!
cmd-keybind-arg-key = <Key>
cmd-keybind-arg-command = "Console Command" e.g. "noise Wurble"
cmd-keybind-save-mode = Save to config? True by default, set to false to clear on reload as long as you don't save elsewhere.
cmd-keybind-error-whitespace = Command can't be blank. Are you trying to remove bindings from the key? Try the unbind command!
cmd-keybind-help = Usage: keybind <Key> <"Console Command"> <Optional: True/False>
    Example: keybind f13 "noise Wurble"

    Unlike the Robust Toolbox bind command, this can be used more than once without relaunching.
    Optionally applies without saving. Saving elsewhere will still include this, so be careful! Try to avoid engine-bound keys here, or you may experience some issues.\n
    _

cmd-unbind-desc = Remove all bindings from a key, and saves it by default. 
cmd-unbind-info-empty = There was nothing to unbind on this key!
cmd-unbind-help = Usage: unbind <Key> <Optional: True/False>
    Example: unbind f13

    Clears all assignments for the specified key. Optionally applies without saving. Saving elsewhere will still include this, so be careful!
    _
