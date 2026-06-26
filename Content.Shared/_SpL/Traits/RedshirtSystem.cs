using Content.Shared._SpL.Medical.Limbs;
using Content.Shared._SpL.Traits;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._SpL.Traits;

public sealed partial class RedShirtSystem : EntitySystem{
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize(){
        SubscribeLocalEvent<RedshirtComponent, ComponentStartup>(SetMobDeathThreshold, after: new[] {typeof(BrittleBonesSystem)});
        SubscribeLocalEvent<RedshirtComponent, ComponentShutdown>(ResetMobstateThresholds, after: new[] {typeof(BrittleBonesSystem)});
    }

    private void SetMobDeathThreshold(Entity<RedshirtComponent> ent, ref ComponentStartup args){
        if (!TryComp(ent.Owner, out MobStateComponent? mobStateComp) ||
            !TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp))
            return;
        
        // halves the death threshold
        var deathThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Dead, mobThreshComp); 
        _mobThreshold.SetMobStateThreshold(ent.Owner, deathThreshold / 2, MobState.Dead, mobThreshComp);

        // if Brittle Bones exists, don't
        if (!HasComp<BrittleBonesComponent>(ent.Owner)) 
            _mobThreshold.SetMobStateThreshold(ent.Owner, -1, MobState.Critical, mobThreshComp);
    }

    private void ResetMobstateThresholds(Entity<RedshirtComponent> ent, ref ComponentShutdown args){
        if (!TryComp(ent.Owner, out MobStateComponent? mobStateComp) ||
            !TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp))
            return;
        
        // doubles the death threshold again.
        var deathThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Dead, mobThreshComp); 
        _mobThreshold.SetMobStateThreshold(ent.Owner, deathThreshold * 2, MobState.Dead, mobThreshComp);

        // crit threshold is like universally half of death and if your species doesn't do that,,, why?
        if (!HasComp<BrittleBonesComponent>(ent.Owner)) 
            _mobThreshold.SetMobStateThreshold(ent.Owner, deathThreshold, MobState.Critical, mobThreshComp);
    }
}