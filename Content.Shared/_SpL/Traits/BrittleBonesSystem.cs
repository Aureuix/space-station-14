using System.Linq;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._SpL.Traits;

public sealed partial class BrittleBonesSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize(){
        SubscribeLocalEvent<BrittleBonesComponent, ComponentStartup>(SetMobCritThreshold, before: new[] { typeof(RedshirtComponent) });
        SubscribeLocalEvent<BrittleBonesComponent, ComponentShutdown>(ResetMobCritThreshold, before: new[] { typeof(RedshirtComponent) });
    }

    private void SetMobCritThreshold(Entity<BrittleBonesComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp) ||
            !TryPrototype(ent, out var proto) ||
            !proto.TryGetComponent<MobThresholdsComponent>(out var protoThreshComp))
            return;

        // Brittle should innately apply before polarity, but this check's here in case it gets VV'd.
        // (even though the stupid handler ordering thing doesn't seem to work, I think it still loads them alphabetically? Maybe not ideal? Worth a revisit later, this should keep it fine either way)
        if (HasComp<RedshirtComponent>(ent.Owner))
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key * 0.5, MobState.SoftCritical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key * 0.5, MobState.Critical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key * 0.5, MobState.Dead, mobThreshComp);
        }
        else
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key * 0.5, MobState.SoftCritical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.Critical).Key * 0.75, MobState.Critical, mobThreshComp);
        }
    }

    private void ResetMobCritThreshold(Entity<BrittleBonesComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp) ||
            !TryPrototype(ent, out var proto) ||
            !proto.TryGetComponent<MobThresholdsComponent>(out var protoThreshComp))
            return;


        if (HasComp<RedshirtComponent>(ent.Owner))
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key, MobState.SoftCritical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.Critical).Key, MobState.Critical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key, MobState.Dead, mobThreshComp);
        }
        else
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key, MobState.SoftCritical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.Critical).Key, MobState.Critical, mobThreshComp);
        }
    }
}