using System.Text;
using Content.Shared.Speech;
using Content.Server.Speech.Components;
using Content.Server._SpL.Speech.Components;
using Content.Server.Speech.EntitySystems;
using System.Text.RegularExpressions;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class RussianAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    [GeneratedRegex(@"\b the\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexThe();

    [GeneratedRegex(@"\b a\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexA();

    [GeneratedRegex(@"\b an\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexAn();
    
    public override void Initialize() 
        => SubscribeLocalEvent<RussianAccentComponent, AccentGetEvent>(OnAccent);

    private void OnAccent(EntityUid uid, RussianAccentComponent component, AccentGetEvent args) 
    {
        args.Message = _replacement.ApplyReplacements(args.Message, "russian");
        
        args.Message.Text = RegexThe().Replace(args.Message.Text, m => PreserveCase(m.Value, ""));
        args.Message.Text = RegexA().Replace(args.Message.Text, m => PreserveCase(m.Value, ""));
        args.Message.Text = RegexAn().Replace(args.Message.Text, m => PreserveCase(m.Value, ""));
    }
    
    private static string PreserveCase(string original, string replacement)
    {
        if (string.IsNullOrEmpty(original))
            return replacement;

        if (char.IsUpper(original[0]))
        {
            return original.Length > 1 && char.IsUpper(original[1])
                ? replacement.ToUpperInvariant()
                : char.ToUpperInvariant(replacement[0]) + replacement[1..];
        }

        return replacement;
    }
}
