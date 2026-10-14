using System.Linq;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._SpL.Traits;

public sealed partial class RedShirtSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RedshirtComponent, ComponentStartup>(SetMobDeathThreshold, after: new[] { typeof(BrittleBonesSystem) });
        SubscribeLocalEvent<RedshirtComponent, ComponentShutdown>(ResetMobstateThresholds, before: new[] { typeof(BrittleBonesSystem) });
    }

    private void SetMobDeathThreshold(Entity<RedshirtComponent> ent, ref ComponentStartup args)
    {

        if (!TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp) ||
            !TryPrototype(ent, out var proto) ||
            !proto.TryGetComponent<MobThresholdsComponent>(out var protoThreshComp))
            return;

        if (HasComp<BrittleBonesComponent>(ent.Owner))
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key * 0.5, MobState.Dead, mobThreshComp);
        }
        else
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key, MobState.Dead, mobThreshComp);

        }
    }

    private void ResetMobstateThresholds(Entity<RedshirtComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp) ||
            !TryPrototype(ent, out var proto) ||
            !proto.TryGetComponent<MobThresholdsComponent>(out var protoThreshComp))
            return;

        if (HasComp<BrittleBonesComponent>(ent.Owner))
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.SoftCritical).Key * 0.5, MobState.SoftCritical, mobThreshComp);
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.Critical).Key * 0.75, MobState.Critical, mobThreshComp);
        }
        else
        {
            _mobThreshold.SetMobStateThreshold(ent.Owner, protoThreshComp.Thresholds.First(thresh => thresh.Value == MobState.Dead).Key, MobState.Dead, mobThreshComp);
        }
    }
}