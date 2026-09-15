using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared.Damage.Prototypes;

/// <summary>
///     A version of DamageModifierSet that can be serialized as a prototype, but is functionally identical.
/// </summary>
/// <remarks>
///     Done to avoid removing the 'required' tag on the ID and passing around a 'prototype' when we really
///     just want normal data to be deserialized.
/// </remarks>
[Prototype]
public sealed partial class DamageModifierSetPrototype : DamageModifierSet, IPrototype, IInheritingPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;
        
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<DamageModifierSetPrototype>))]
    public string[]? Parents { get; private set; }
        
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }
}