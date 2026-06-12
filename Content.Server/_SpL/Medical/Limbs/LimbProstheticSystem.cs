using System.Linq;
using Content.Shared._SpL.Medical.Limbs;
using Content.Server._Starlight.Medical.Limbs;
using Content.Shared.Interaction;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Server.Body.Systems;  
using Content.Shared.DoAfter;

using Content.Shared.Verbs;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using Robust.Shared.Containers;

using System;
using System.Reflection;
using System.Diagnostics.Tracing;

namespace Content.Server._SpL.Medical.Limbs;

public sealed partial class LimbProstheticSystem : EntitySystem {
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly LimbSystem _limbSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    private EntityQuery<BodyPartComponent> _bodyPartQuery;

    public override void Initialize(){
        SubscribeLocalEvent<LimbProstheticComponent, LimbInitializedEvent>(OnLimbInit);
        SubscribeLocalEvent<LimbProstheticComponent, AfterInteractEvent>(EquipDoAfterOnInteract);
        SubscribeLocalEvent<LimbProstheticComponent, EquipProstheticDoAfterEvent>(EquipProstheticAfterInteract);

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(AddProstheticVerbs);
        SubscribeLocalEvent<LimbProstheticComponent, RemoveProstheticDoAfterEvent>(TryRemovalAfterInteract);

        SubscribeLocalEvent<LimbProstheticComponent, LimbDetachedEvent>(RemoveProstheticPartFromShared);

        _bodyPartQuery = GetEntityQuery<BodyPartComponent>();
    }

    #region Initalizing

    private void OnLimbInit(Entity<LimbProstheticComponent> entity, ref LimbInitializedEvent args){   
        if (!TryComp(entity.Owner, out BodyPartComponent? bodyPartComp)) return;
        if (!bodyPartComp.Body.HasValue) return;

        DeleteSuperfluousChildrenOnInit(entity, bodyPartComp);
        DeleteSuperfluousContainers(bodyPartComp.Body.Value);
        AddProstheticPartToShared(entity, bodyPartComp.Body.Value);
    }

    //deletes limbs and containers that shouldn't be there 
    private void DeleteSuperfluousChildrenOnInit(Entity<LimbProstheticComponent> entity, BodyPartComponent part){
        if (entity.Comp.hasChildren) return;

        if (!TryComp(entity.Owner, out BodyPartComponent? bodyPartComp)) return;

        if (!TryComp(part.Body, out BodyComponent? bodyComp) ||
            !TryComp(part.Body, out TransformComponent? transform) ||
            !TryComp(part.Body, out HumanoidAppearanceComponent? appearance)) return; 

        if (!part.Body.HasValue) return;
        EntityUid bodyId = part.Body ?? entity.Owner;
        Entity<TransformComponent, HumanoidAppearanceComponent, BodyComponent> body = (bodyId, transform, appearance, bodyComp);

        var allChildren = _bodySystem.GetBodyPartChildren(entity, bodyPartComp);

        foreach (var child in allChildren){
            if (child.Id == entity.Owner ||
                !TryComp(child.Id, out TransformComponent? childTransform) ||
                !TryComp(child.Id, out MetaDataComponent? childMetadata) ||
                !TryComp(child.Id, out BodyPartComponent? childBodyPart)) continue;

            Entity<TransformComponent, MetaDataComponent, BodyPartComponent> childToDelete = (child.Id, childTransform, childMetadata, childBodyPart);
            _limbSystem.Amputatate(body, childToDelete); // <- errors because we have to make Body not null for the function but the assertion checks against it being null and fails if it isn't 
            Del(child.Id);
        }
    }

    private void DeleteSuperfluousContainers(EntityUid body){

        // Commented out until i fix the inventory system spamming the console with errors about missing containers when trying to equip shoes/gloves.

        /* if (!TryComp(body, out ContainerManagerComponent? containerManager)) return;
        //Check if body has hands or feet, remove containers accordingly if absent.
        if (!_bodySystem.BodyHasPartType(body, BodyPartType.Hand) && _containerSystem.HasContainer(body, "gloves", containerManager)){
            _containerSystem.ShutdownContainer(_containerSystem.GetContainer(body, "gloves", containerManager));
        } 
        if (!_bodySystem.BodyHasPartType(body, BodyPartType.Foot) && _containerSystem.HasContainer(body, "shoes", containerManager)){
            _containerSystem.ShutdownContainer(_containerSystem.GetContainer(body, "shoes", containerManager));
        } 

        Dirty(body, containerManager);*/
    }

    #endregion

    #region Equipping

    private void EquipDoAfterOnInteract(Entity<LimbProstheticComponent> entity, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled || args.Target == null) return;

        EntityUid target = args.Target.GetValueOrDefault(default!);
        if(!TryComp(target, out HumanoidAppearanceComponent? appearance) ||
           !TryComp(target, out BodyComponent? bodyComp) ||
           !TryComp(entity, out BodyPartComponent? partComp))
           return;

        var doAfter =
            new DoAfterArgs(EntityManager, args.User, 6, new EquipProstheticDoAfterEvent(), entity, target: target, used: entity)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
            };


        if (_bodySystem.BodyHasPartTypeOfSymmetry(target, partComp.PartType, partComp.Symmetry, bodyComp)){
            _popupSystem.PopupEntity("Can fit prosthetic over existing limb", args.User, PopupType.Medium);
            return;
        }

        _doAfterSystem.TryStartDoAfter(doAfter);
    }

    private void EquipProstheticAfterInteract(Entity<LimbProstheticComponent> entity, ref EquipProstheticDoAfterEvent args){
        EntityUid target = args.Target.GetValueOrDefault(default!);
        if(!TryComp(target, out HumanoidAppearanceComponent? appearance) ||
           !TryComp(target, out BodyComponent? bodyComp) ||
           !TryComp(entity, out BodyPartComponent? partComp))
           return;

        EnsureComp<BodyComponent>(target);

        //If target already has limb of same type as prosthetic, prevent equipping and exit function;
        if (!_bodySystem.BodyHasPartTypeOfSymmetry(target, partComp.PartType, partComp.Symmetry, bodyComp)){ 
            var torso = _bodySystem.GetRootPartOrNull(target, bodyComp);
            if (torso is not null){
                // Bugged: losing both arms or legs somehow deletes the bodypartslot for them and prevents prosthetics from being equipped
                var slot = CyberneticImplant.SlotIDFromBodypart(partComp);
                IEnumerable<Entity<BodyPartComponent>> _allParts = _bodySystem.GetAllBodyPart(torso.Value.Entity, torso.Value.BodyPart);
                foreach(var part in _allParts){
                    if(!_bodyPartQuery.TryComp(part, out BodyPartComponent? comp)) return;

                    IEnumerable<BodyPartSlot> slots = _bodySystem.GetAllBodyPartSlots(part, comp);

                    foreach (var subPart in slots){
                        if (subPart.Id != slot || subPart.Id == null) continue;
                        _limbSystem.AttachLimb((target, appearance), slot, part, (entity, partComp));
                        AddProstheticPartToShared(entity, target);
                        return;
                    }
                }
                _limbSystem.AttachLimb((target, appearance), slot, (torso.Value.Entity, torso.Value.BodyPart), (entity, partComp));
                AddProstheticPartToShared(entity, target);
            }
            return;
        }
        args.Handled = true;
    }

    #endregion

    #region Unequipping

    private void AddProstheticVerbs(GetVerbsEvent<Verb> args){

        //Check if target is wearing a prosthetic limb in the first place.
        if(!TryComp<BodyComponent>(args.Target, out var body) || body.ProstheticParts.Count <= 0){
            return;
        }
        //create a verb subcategory
        var category = new VerbCategory("prosthetic-verb-get-data-text", "/Textures/Interface/VerbIcons/examine.svg.192dpi.png");

        foreach (var prosthetic in body.ProstheticParts){
            if (!TryComp(prosthetic, out MetaDataComponent? metaData) ||
                !TryComp<LimbProstheticComponent>(prosthetic, out var limbProsthetic)) continue;

            Verb verb = new()
            {
                Text = metaData.EntityName,
                Icon = null,
                Category = category,
                Act = () => RemovalDoAfterOnInteract(prosthetic, args.Target, args.User)
            };
            args.Verbs.Add(verb);
        }
    }

    private void RemovalDoAfterOnInteract(Entity<LimbProstheticComponent?> entity, EntityUid target, EntityUid user)
    {
        if (!Resolve(entity.Owner, ref entity.Comp)) return;
        if (!TryComp(entity.Owner, out BodyPartComponent? partComp) ||
            !TryComp(entity.Owner, out MetaDataComponent? partMeta) ||
            !TryComp(user, out MetaDataComponent? userMeta)) return;

        // Checks if the player isn't trying to remove their only remaining hand
        if (!_bodySystem.BodyHasPartTypeOfSymmetry(user, partComp.PartType, FlipSymmetry(partComp.Symmetry)) && 
            target == user){
                if (partComp.PartType == BodyPartType.Hand || partComp.PartType == BodyPartType.Arm){
                    _popupSystem.PopupEntity("You can't remove your only remaining hand by yourself!", target, PopupType.Medium);
                    return;
                }
        }
        
        //REMOVAL DOAFTER
        var doAfter =
            new DoAfterArgs(EntityManager, user, 6f, new RemoveProstheticDoAfterEvent(GetNetEntity(target), GetNetEntity(user), GetNetEntity(entity)), entity, target: target, used: entity)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                BreakOnDropItem = true,
                NeedHand = true,
                RequireCanInteract = true,
            };

        if (target != user){
            _popupSystem.PopupEntity($"{userMeta.EntityName.ToLower()} is trying to take off your {partMeta.EntityName.ToLower()}!", target, PopupType.MediumCaution);
        } else {
            _popupSystem.PopupEntity($"you begin to take off your {partMeta.EntityName.ToLower()}!", target, PopupType.Medium);
        }
        
        _doAfterSystem.TryStartDoAfter(doAfter);
    }

    private void TryRemovalAfterInteract(Entity<LimbProstheticComponent> entity, ref RemoveProstheticDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null) return;
        if (!TryGetEntity(args.Target, out EntityUid? target) ||
            !TryGetEntity(args.User, out EntityUid? user)) return;

        if (!TryComp(target, out TransformComponent? transform) ||
            !TryComp(target, out HumanoidAppearanceComponent? appearance) ||
            !TryComp(target, out BodyComponent? bodyComp) ||
            !TryComp(entity, out BodyPartComponent? partComp))
            return;
        
        var targetLimb = _bodySystem.GetBodyChildrenOfType(target.Value, partComp.PartType).FirstOrDefault(p => p.Component.Symmetry == partComp.Symmetry);
        if (!TryComp(targetLimb.Id, out TransformComponent? targetLimbTransform) ||
            !TryComp(targetLimb.Id, out MetaDataComponent? targetLimbMetadata) ||
            !TryComp(targetLimb.Id, out BodyPartComponent? targetLimbBodyPart))
            return;

        Entity<TransformComponent, HumanoidAppearanceComponent, BodyComponent> body = (target.Value, transform, appearance, bodyComp);
        Entity<TransformComponent, MetaDataComponent, BodyPartComponent> limbToRemove = (targetLimb.Id, targetLimbTransform, targetLimbMetadata, targetLimbBodyPart);

        _limbSystem.Amputatate(body, limbToRemove); 
        _handSystem.TryPickup(user.Value, limbToRemove);
    }

    #endregion

    #region Queries

    //  We're doing this because Starlight put all of the limb system serverside so we need an intermediary to communicate with the client...
    public void AddProstheticPartToShared(Entity<LimbProstheticComponent> part, EntityUid entity)
    {
        if (!TryComp(entity, out BodyComponent? bodyComp)) return;

        bodyComp.ProstheticParts.Add(part);

        Dirty(entity, bodyComp);
    }

    public void RemoveProstheticPartFromShared(Entity<LimbProstheticComponent> part, ref LimbDetachedEvent args)
    {
        if (!TryComp(args.Body, out BodyComponent? bodyComp)) return;

        if (!bodyComp.ProstheticParts.Contains(part.Owner)) return;
        
        bodyComp.ProstheticParts.Remove(part.Owner);
        
        Dirty(bodyComp.Owner, bodyComp);
    }

    private BodyPartSymmetry FlipSymmetry(BodyPartSymmetry symmetry){
        if (symmetry == BodyPartSymmetry.Left){
            return BodyPartSymmetry.Right;
        }
        else if (symmetry == BodyPartSymmetry.Right){
            return BodyPartSymmetry.Left;
        }
        else return BodyPartSymmetry.None;
    }

    #endregion
}