using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Starlight.DeviceLinking.Components;

[RegisterComponent]
public sealed partial class GasMixerMolarSignalComponent : Component
{
    [DataField("onPort", customTypeSerializer: typeof(ProtoId<SinkPortPrototype>))]
    public string OnPort = "On";

    [DataField("offPort", customTypeSerializer: typeof(ProtoId<SinkPortPrototype>))]
    public string OffPort = "Off";

    [DataField("togglePort", customTypeSerializer: typeof(ProtoId<SinkPortPrototype>))]
    public string TogglePort = "Toggle";
}