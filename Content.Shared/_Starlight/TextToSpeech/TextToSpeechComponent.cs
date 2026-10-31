using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Starlight.TextToSpeech;

[RegisterComponent, NetworkedComponent]
public sealed partial class TextToSpeechComponent : Component
{
    [DataField("voice", customTypeSerializer: typeof(ProtoId<VoicePrototype>))]
    public string? VoicePrototypeId { get; set; }
}
