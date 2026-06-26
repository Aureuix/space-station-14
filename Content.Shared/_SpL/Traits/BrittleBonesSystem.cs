using Content.Shared._SpL.Traits;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared._SpL.Traits;

public sealed partial class BrittleBonesSystem : EntitySystem {
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize(){
        SubscribeLocalEvent<BrittleBonesComponent, ComponentStartup>(SetMobCritThreshold);
        SubscribeLocalEvent<BrittleBonesComponent, ComponentShutdown>(ResetMobCritThreshold);
    }

    private void SetMobCritThreshold(Entity<BrittleBonesComponent> ent, ref ComponentStartup args){
        if (!TryComp(ent.Owner, out MobStateComponent? mobStateComp) ||
            !TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp))
            return;
        
        // halves the death threshold
        if (!HasComp<RedshirtComponent>(ent.Owner)){
            var critThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Critical, mobThreshComp); 
            _mobThreshold.SetMobStateThreshold(ent.Owner, critThreshold / 2, MobState.Critical, mobThreshComp);
        } else {
            var deathThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Dead, mobThreshComp); 
            _mobThreshold.SetMobStateThreshold(ent.Owner, deathThreshold / 2, MobState.Critical, mobThreshComp);
        }
    }

    private void ResetMobCritThreshold(Entity<BrittleBonesComponent> ent, ref ComponentShutdown args){
        if (!TryComp(ent.Owner, out MobStateComponent? mobStateComp) ||
            !TryComp(ent.Owner, out MobThresholdsComponent? mobThreshComp))
            return;

        var critThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Critical, mobThreshComp);
        if (!HasComp<RedshirtComponent>(ent.Owner)){
            _mobThreshold.SetMobStateThreshold(ent.Owner, critThreshold * 2, MobState.Critical, mobThreshComp);
        } else {
            _mobThreshold.SetMobStateThreshold(ent.Owner, critThreshold * 2 - 1, MobState.Critical, mobThreshComp);
        }
    }
}