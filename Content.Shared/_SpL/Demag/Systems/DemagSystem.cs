using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.Demag.Components;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Demag.Systems;

public sealed class DemagSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemagComponent, AfterInteractEvent>(OnAfterInteract);
    }
    
    private void OnAfterInteract(EntityUid uid, DemagComponent comp, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        args.Handled = TryDemagEffect((uid, comp), args.User, target);
    }
    // TODO: make the demag able to reset subverted borgs too. clear subvertedsilicon, remove corrupted laws and gained channels and restore crewsimov
    
    /// <summary>
    /// handles removing the components associated with emagging behaviour
    /// </summary>
    public bool TryDemagEffect(Entity<DemagComponent?> ent, EntityUid user, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;
        
        // don't demag if there's no charges left
        Entity<LimitedChargesComponent?> chargesEnt = ent.Owner;
        if (_sharedCharges.IsEmpty(chargesEnt))
        {
            _popup.PopupClient(Loc.GetString("emag-no-charges"), user, user);
            return false;
        }
        
        //remove the emag component if it exists
        if (HasComp<EmaggedComponent>(target))
        {
            RemComp<EmaggedComponent>(target);

            _popup.PopupPredicted(Loc.GetString("demag-success"), user, user, PopupType.Medium);

            _audio.PlayPredicted(ent.Comp.DemagSound, ent, ent);

            _adminLogger.Add(LogType.Emag, LogImpact.High,
                $"{ToPrettyString(user):player} de-emagged {ToPrettyString(target):target}");
            
            _sharedCharges.TryUseCharge(chargesEnt);
        } 
        
        // report that there was nothing to remove if the target isn't emagged
        else
        {
            _popup.PopupPredicted(Loc.GetString("demag-failure"), user, user, PopupType.Medium);
        }
        return false;
    }
}