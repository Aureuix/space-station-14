using Content.Shared.Verbs;
using Content.Shared._SpL.Medical.Limbs;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using System.Runtime.CompilerServices;
using Content.Shared.Body.Components;
using System.Linq;


namespace Content.Shared._SpL.Medical.Limbs;

public sealed partial class LimbProstheticSystem : EntitySystem {
    public override void Initialize(){
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(AddProstheticVerbs);
    }

    private void AddProstheticVerbs(GetVerbsEvent<Verb> args){

        //Check if target is wearing a prosthetic limb in the first place.
        if(!TryComp(args.Target, out BodyComponent? body) || body.ProstheticParts.Count !> 0){
            return;
        }
        //create a verb subcategory
        var category = new VerbCategory("Check Prosthetics", null);

        foreach (var prosthetic in body.ProstheticParts){
            Verb verb = new()
            {
                Text = Loc.GetString("prosthetic-verb-get-data-text"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/examine.svg.192dpi.png")),
                Category = category,
                ClientExclusive = true,
                Act = () => RaiseNetworkEvent(new RemoveProstheticEvent(GetNetEntity(args.Target)))
            };
            args.Verbs.Add(verb);
        }
        
    }
}