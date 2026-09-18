using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class NoiseCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IEntityManager _entities = default!;
        public override string Command => "noise";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _prototypeManager.EnumeratePrototypes<EmotePrototype>();
            if (shell.Player is not { } player)
            {
                shell.WriteError(Loc.GetString($"shell-cannot-run-command-from-server"));
                return;
            }

            if (player.Status != SessionStatus.InGame)
                return;

            if (player.AttachedEntity is not {} playerEntity)
            {
                shell.WriteError(Loc.GetString($"shell-must-be-attached-to-entity"));
                return;
            }
            if (args.Length > 0)
            {
                if (_prototypeManager.Resolve<EmotePrototype>(args[0], out var proto))
                {
                    _chatSystem.TryEmoteWithoutChat(player.AttachedEntity ?? EntityUid.Invalid, proto);
                }
            }
        }
        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (shell.Player is not { } player || player.Status != SessionStatus.InGame || player.AttachedEntity is not { } playerEntity)
            {
                return CompletionResult.Empty;
            }
            if (args.Length < 2)
            {
                List<string> validEmotes = [];

                if (!_entities.TryGetComponent(playerEntity, out VocalComponent? vocal) || vocal.EmoteSounds is not { } noiseID || !_prototypeManager.TryIndex(noiseID, out EmoteSoundsPrototype? sounds))
                {
                    return CompletionResult.Empty;
                }

                var prototypes = _prototypeManager.EnumeratePrototypes<EmotePrototype>();
                foreach (EmotePrototype emote in prototypes)
                {
                    if (emote.Category == EmoteCategory.Vocal && sounds.Sounds.ContainsKey(emote.ID) && _chatSystem.AllowedToUseEmote(playerEntity, emote))
                    {
                        validEmotes.Add(emote.ID);
                    }
                }
                return CompletionResult.FromHintOptions(validEmotes, Loc.GetString("cmd-noise-hints"));
            }
            return CompletionResult.Empty;
        }
    }
}
