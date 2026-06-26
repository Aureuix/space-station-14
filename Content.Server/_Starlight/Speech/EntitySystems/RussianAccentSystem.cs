using System.Text;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed class RussianAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize() 
        => SubscribeLocalEvent<RussianAccentComponent, AccentGetEvent>(OnAccent);

    public SpeechMessage Accentuate(SpeechMessage message)
    {
        message = _replacement.ApplyReplacements(message, "russian");
        
        return message;
    }

    private void OnAccent(EntityUid uid, RussianAccentComponent component, AccentGetEvent args) 
        => args.Message = Accentuate(args.Message);
}
