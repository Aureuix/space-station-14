using Robust.Shared.Audio;

namespace Content.Shared.Demag.Components;

[RegisterComponent]
public sealed partial class DemagComponent : Component
{
    /// <summary>
    /// gives a sound for DemagSystem to reference 
    /// </summary>
    [DataField]
    public SoundSpecifier DemagSound = new SoundCollectionSpecifier("Keyboard");
}