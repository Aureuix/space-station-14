using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared._SpL.Medical.Limbs;

// One of these is causing Serializable to crash on build :sobs:
[Serializable, NetSerializable]
public sealed partial class RemoveProstheticDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity Target;
    public NetEntity User;
    public NetEntity Prosthetic;
     
    public RemoveProstheticDoAfterEvent(NetEntity target, NetEntity user, NetEntity prosthetic){
        Target = target;
        User = user;
        Prosthetic = prosthetic;
    }
}

[Serializable, NetSerializable]
public sealed partial class EquipProstheticDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class LimbInitializedEvent : SimpleDoAfterEvent
{
}