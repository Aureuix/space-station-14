using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Preferences;
using Content.Shared.Humanoid;
using Content.Server.Humanoid;
using Content.Server._Starlight.Medical.Limbs;
using Content.Shared._Starlight.Medical.Body;
using Content.Server._SpL.Traits.Assorted;
using Content.Server.Body.Systems;  
using Content.Server.Station.Systems;
using Robust.Shared.Prototypes;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Contant.Server._SpL.Traits.Assorted;
public sealed class AmputeeSystem : EntitySystem
{
    [Dependency] private readonly LimbSystem _limbSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<AmputeeComponent, MapInitEvent>(AmputateLimbs);
    }

    // TODO: - Make the amputee subtraits actually affect the component before AmputateLimbs() runs.
    //       - Make it so limbs are fully deleted instead of having an item spawn on the ground.
    //       - Make it so characters can spawn with prosthetic limbs -> implement prosthetic limbs that aren't as good as cybernetics.  
    private void AmputateLimbs(Entity<AmputeeComponent> entity, ref MapInitEvent args) 
    {   
        if (!TryComp(entity.Owner, out TransformComponent? transform) ||
            !TryComp(entity.Owner, out HumanoidAppearanceComponent? appearance) ||
            !TryComp(entity.Owner, out BodyComponent? bodyComp) ||
            !TryComp(entity.Owner, out AmputeeComponent? ampComp))
        {
            Log.Debug("Can't find crucial components");
            return;
        };

        Entity<TransformComponent, HumanoidAppearanceComponent, BodyComponent> body = (entity.Owner, transform, appearance, bodyComp);

        var root = _bodySystem.GetRootPartOrNull(entity.Owner, bodyComp);
        if (root != null){
            IEnumerable<Entity<BodyPartComponent>> _allLimbs = _bodySystem.GetAllBodyPart(root.Value.Entity, root.Value.BodyPart);
            foreach (var bodyPart in _allLimbs)
            { 
                if (!TryComp(bodyPart, out BodyPartComponent? bodyPartComp)){
                    continue;
                }

                var targetLimb = _bodySystem.GetBodyChildrenOfType(entity.Owner, bodyPartComp.PartType).FirstOrDefault(p => p.Component.Symmetry == bodyPartComp.Symmetry);
                if (!TryComp(targetLimb.Id, out TransformComponent? targetLimbTransform) ||
                    !TryComp(targetLimb.Id, out MetaDataComponent? targetLimbMetadata) ||
                    !TryComp(targetLimb.Id, out BodyPartComponent? targetLimbBodyPart))
                    {
                        Log.Debug("Can't find component");
                        continue;
                    }

                Entity<TransformComponent, MetaDataComponent, BodyPartComponent> limbToDelete = (targetLimb.Id, targetLimbTransform, targetLimbMetadata, targetLimbBodyPart);

                // Unrobust hack, doesn't know how to deal with characters with theoretically more than 2 limbs with the same Type and Symmetry.
                switch (bodyPartComp.PartType){ //Deletes Limbs
                    case BodyPartType.Arm:
                        if (ampComp.missingLeftArm && bodyPartComp.Symmetry == BodyPartSymmetry.Left)   {_limbSystem.Amputatate(body, limbToDelete); Del(targetLimb.Id); break;}
                        if (ampComp.missingRightArm && bodyPartComp.Symmetry == BodyPartSymmetry.Right) {_limbSystem.Amputatate(body, limbToDelete); Del(targetLimb.Id); break;}
                        break;
                    case BodyPartType.Leg:
                        if (ampComp.missingLeftLeg && bodyPartComp.Symmetry == BodyPartSymmetry.Left)   {_limbSystem.Amputatate(body, limbToDelete); Del(targetLimb.Id); break;}
                        if (ampComp.missingRightLeg && bodyPartComp.Symmetry == BodyPartSymmetry.Right) {_limbSystem.Amputatate(body, limbToDelete); Del(targetLimb.Id); break;}
                        break;
                    default:
                        break;    
                }

                
            }
        };
    }
}