using Content.Server._SpL.Speech.EntitySystems;

namespace Content.Server._SpL.Speech.Components;

/// <summary>
/// Avali accent replaces spoken letters. "f" becomes "hth", "v" becomes "bu", and "n" becomes "'h".
/// </summary>
[RegisterComponent]
[Access(typeof(AvaliAccentSystem))]
public sealed partial class AvaliAccentComponent : Component
{

}
