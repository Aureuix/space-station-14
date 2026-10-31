using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines
{
    [Prototype]
    public sealed partial class VendingMachineInventoryPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("startingInventory", customTypeSerializer:typeof(ProtoId<EntityPrototype>))]
        public Dictionary<EntProtoId, uint> StartingInventory { get; private set; } = new();

        [DataField("emaggedInventory", customTypeSerializer:typeof(ProtoId<EntityPrototype>))]
        public Dictionary<EntProtoId, uint>? EmaggedInventory { get; private set; }

        [DataField("contrabandInventory", customTypeSerializer:typeof(ProtoId<EntityPrototype>))]
        public Dictionary<EntProtoId, uint>? ContrabandInventory { get; private set; }
    }
}
