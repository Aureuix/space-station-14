using Content.Shared.Verbs;
using Content.Shared._SpL.Medical.Limbs;
using Content.Server._Starlight.Medical.Limbs;
using Content.Shared.Interaction;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Popups;
using Content.Shared.Humanoid;
using Content.Server.Humanoid;
using Content.Server.Body.Systems;  
using Content.Shared.Timing;
using Content.Shared.DoAfter;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Robust.Shared.GameObjects;
using System;
using System.Reflection;
using System.Diagnostics;
using Content.Shared.Starlight.Medical.Surgery.Steps.Parts;

namespace Content.Server._SpL.Medical.Limbs;

public sealed partial class LimbProstheticSystem : EntitySystem {
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly LimbSystem _limbSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize(){
        SubscribeLocalEvent<LimbProstheticComponent, LimbInitializedEvent>(DeleteSuperfluousChildrenOnInit);
        SubscribeLocalEvent<LimbProstheticComponent, AfterInteractEvent>(EquipDoAfterOnInteract);
        SubscribeLocalEvent<LimbProstheticComponent, EquipProstheticDoAfterEvent>(EquipProstheticAfterInteract);
    }

    #region Initalizing

    public void DeleteSuperfluousChildrenOnInit(Entity<LimbProstheticComponent> entity, ref LimbInitializedEvent args){
        if (entity.Comp.hasChildren) return;

        if (!TryComp(entity.Owner, out BodyPartComponent? bodyPartComp)) return;

        if (!TryComp(bodyPartComp.Body, out BodyComponent? bodyComp) ||
            !TryComp(bodyPartComp.Body, out TransformComponent? transform) ||
            !TryComp(bodyPartComp.Body, out HumanoidAppearanceComponent? appearance)) return; 

        if (!bodyPartComp.Body.HasValue) return;
        EntityUid bodyId = bodyPartComp.Body ?? entity.Owner;
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

        //If target already has limb of same type as prosthetic, prevent equipping and exit function;
        if (!_bodySystem.BodyHasPartTypeOfSymmetry(target, partComp.PartType, partComp.Symmetry, bodyComp)){ 
            var root = _bodySystem.GetRootPartOrNull(target, bodyComp);
            if (root is not null){
                IEnumerable<Entity<BodyPartComponent>> _allSlots = _bodySystem.GetAllBodyPart(root.Value.Entity, root.Value.BodyPart);
                foreach(var part in _allSlots){
                    if (!TryComp(part, out BodyPartComponent? comp)) continue;

                    IEnumerable<BodyPartSlot> slots = _bodySystem.GetAllBodyPartSlots(part, comp);

                    foreach (var subPart in slots){
                        if (subPart.Type != partComp.PartType) continue;
                        if (subPart.Id == null) continue;
                    
                        //REALLY BAD CODING PRACTICE, FIGURE OUT HOW TO NOT HAVE TO ASSUME TORSO LATER -RK
                        var subslot = CyberneticImplant.SlotIDFromBodypart(partComp);
                        _limbSystem.AttachLimb((target, appearance), subslot, part, (entity, partComp));
                    }

                    if (comp.PartType != partComp.PartType) continue;

                    var slot = CyberneticImplant.SlotIDFromBodypart(partComp);
                    _limbSystem.AttachLimb((target, appearance), slot, (root.Value.Entity, root.Value.BodyPart), (entity, partComp));
                }
            }
            return;
        }
        args.Handled = true;
    }

    #endregion

    #region Queries

    public void SendProstheticPartsToShared(Entity<BodyComponent> entity, ref MapInitEvent args){
        /*if(!TryComp(entity, out BodyPartComponent? partComp)) return;
        if(!TryComp(partComp.Body, out BodyComponent? bodyComp)) return;
        IEnumerable<EntityUid> parts = GetProsthetics(entity);
        
        bodyComp.ProstheticParts.Clear();
        foreach(var part in parts){
            bodyComp.ProstheticParts.Add(part);
        }*/
    }

    public IEnumerable<EntityUid> GetProsthetics(EntityUid entity) 
    {   
        if (!TryComp(entity, out BodyComponent? bodyComp)) yield break;

        var root = _bodySystem.GetRootPartOrNull(entity, bodyComp);
        if (root != null){
            IEnumerable<Entity<BodyPartComponent>> _allLimbs = _bodySystem.GetAllBodyPart(root.Value.Entity, root.Value.BodyPart);
            foreach (var bodyPart in _allLimbs)
            { 
                yield return bodyPart;
            }
        };
    }

    #endregion
}