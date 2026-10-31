using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.NPC.Queries.Curves;

public sealed partial class PresetCurve : IUtilityCurve
{
    [DataField("preset", required: true, customTypeSerializer:typeof(ProtoId<UtilityCurvePresetPrototype>))] public  string Preset = default!;
}
