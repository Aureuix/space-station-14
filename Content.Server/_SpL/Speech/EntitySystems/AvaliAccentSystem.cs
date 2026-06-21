using Content.Shared.Speech;
using Content.Server.Speech.Components;
using Content.Server._SpL.Speech.Components;
using Content.Server.Speech.EntitySystems;
using System.Text.RegularExpressions;

namespace Content.Server._SpL.Speech.EntitySystems;

public sealed partial class AvaliAccentSystem : EntitySystem
{    
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    
    [GeneratedRegex(@"f", RegexOptions.IgnoreCase)]
    private static partial Regex RegexF();

    [GeneratedRegex(@"v", RegexOptions.IgnoreCase)]
    private static partial Regex RegexV();

    [GeneratedRegex(@"n", RegexOptions.IgnoreCase)]
    private static partial Regex RegexN();
    
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AvaliAccentComponent, AccentGetEvent>(OnAccent);
    }
    
    private void OnAccent(EntityUid uid, AvaliAccentComponent component, AccentGetEvent args)
    {
        args.Message = _replacement.ApplyReplacements(args.Message, "avali");
        
        args.Message.Text = RegexF().Replace(args.Message.Text, m => PreserveCase(m.Value, "th"));
        args.Message.Text = RegexV().Replace(args.Message.Text, m => PreserveCase(m.Value, "b"));
        args.Message.Text = RegexN().Replace(args.Message.Text, m => PreserveCase(m.Value, "'"));
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
