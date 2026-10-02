using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
// using Content.Server.Zombies; // SpL
using Content.Shared.Administration;
using Content.Server.Clothing.Systems;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Roles.Components;
using Content.Server._Starlight.GameTicking.Rules.Components;
using Content.Shared._Starlight.Shadekin;
using Content.Server.Speech.Components; // Starlight
using Robust.Shared.Audio; // Starlight

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    // [Dependency] private readonly ZombieSystem _zombie = default!; // SpL
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly OutfitSystem _outfit = default!;

    private static readonly EntProtoId DefaultTraitorRule = "Traitor";
    // private static readonly EntProtoId DefaultInitialInfectedRule = "Zombie"; // SpL
    private static readonly EntProtoId DefaultNukeOpRule = "LoneOpsSpawn";
    private static readonly EntProtoId DefaultRevsRule = "Revolutionary";
    private static readonly EntProtoId DefaultThiefRule = "Thief";
    private static readonly EntProtoId DefaultChangelingRule = "Changeling";
    private static readonly EntProtoId ParadoxCloneRuleId = "ParadoxCloneSpawn";
    private static readonly EntProtoId DefaultWizardRule = "Wizard";
    private static readonly EntProtoId DefaultNinjaRule = "NinjaSpawn";
    private static readonly ProtoId<StartingGearPrototype> PirateGearId = "PirateGear";
    private static readonly EntProtoId DefaultVampireRule = "Vampire"; //Starlight
    private static readonly EntProtoId DefaultBrighteyeRule = "Brighteye"; //Starlight
	private static readonly EntProtoId DefaultSELFRule = "SiliconLiberation"; //Starlight
    #region SpL
    // SpL- general beaming is done via this system also. it's clunky but what works works
    // make sure you add the component and gear even if the added beam option isn't an antag
    private static readonly EntProtoId DefaultCentcommRule = "CentCommSpawn";
    private static readonly ProtoId<StartingGearPrototype> CentCommGearID = "CentCommOpGear";
    private static readonly EntProtoId DefaultCBURNRule = "CBURNSpawn";
    private static readonly ProtoId<StartingGearPrototype> CBURNGearID = "CBURNGearCleanup";
    private static readonly EntProtoId DefaultCBURNAltRule = "CBURNAltSpawn";
    private static readonly ProtoId<StartingGearPrototype> CBURNGearAltID = "CBURNGearLiquidator";
    private static readonly EntProtoId DefaultNTNCRule = "NTNCSpawn";
    private static readonly ProtoId<StartingGearPrototype> NTNCGearID = "NTNCGearBasic";
    #endregion

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return;

        var targetPlayer = targetActor.PlayerSession;

        var traitorName = Loc.GetString("admin-verb-text-make-traitor");
        Verb traitor = new()
        {
            Text = traitorName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/job_icons.rsi"), "Syndicate"),
            Act = () =>
            {
                _antag.ForceMakeAntag<TraitorRuleComponent>(targetPlayer, DefaultTraitorRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", traitorName, Loc.GetString("admin-verb-make-traitor")),
        };
        args.Verbs.Add(traitor);

        /* var initialInfectedName = Loc.GetString("admin-verb-text-make-initial-infected");
        Verb initialInfected = new()
        {
            Text = initialInfectedName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "InitialInfected"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ZombieRuleComponent>(targetPlayer, DefaultInitialInfectedRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", initialInfectedName, Loc.GetString("admin-verb-make-initial-infected")),
        };
        args.Verbs.Add(initialInfected); */ //SpL- death to initial infected

        /* var zombieName = Loc.GetString("admin-verb-text-make-zombie");
        Verb zombie = new()
        {
            Text = zombieName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "Zombie"),
            Act = () =>
            {
                _zombie.ZombifyEntity(args.Target);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", zombieName, Loc.GetString("admin-verb-make-zombie")),
        };
        args.Verbs.Add(zombie); */ // SpL- removed zombie from the smite menu TODO- move this to smite menu

        var nukeOpName = Loc.GetString("admin-verb-text-make-nuclear-operative");
        Verb nukeOp = new()
        {
            Text = nukeOpName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hardsuits/syndicate.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<NukeopsRuleComponent>(targetPlayer, DefaultNukeOpRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", nukeOpName, Loc.GetString("admin-verb-make-nuclear-operative")),
        };
        args.Verbs.Add(nukeOp);

        /* var pirateName = Loc.GetString("admin-verb-text-make-pirate") + " (Wizden)"; // Starlight
        Verb pirate = new()
        {
            Text = pirateName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                // pirates just get an outfit because they don't really have logic associated with them
                _outfit.SetOutfit(args.Target, PirateGearId);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", pirateName, Loc.GetString("admin-verb-make-pirate")),
        };
        args.Verbs.Add(pirate); */ // SpL- removess wizden pirates

        var headRevName = Loc.GetString("admin-verb-text-make-head-rev");
        Verb headRev = new()
        {
            Text = headRevName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "HeadRevolutionary"),
            Act = () =>
            {
                _antag.ForceMakeAntag<RevolutionaryRuleComponent>(targetPlayer, DefaultRevsRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", headRevName, Loc.GetString("admin-verb-make-head-rev")),
        };
        args.Verbs.Add(headRev);

        var thiefName = Loc.GetString("admin-verb-text-make-thief");
        Verb thief = new()
        {
            Text = thiefName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Clothing/Hands/Gloves/Color/black.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ThiefRuleComponent>(targetPlayer, DefaultThiefRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", thiefName, Loc.GetString("admin-verb-make-thief")),
        };
        args.Verbs.Add(thief);

        /*var changelingName = Loc.GetString("admin-verb-text-make-changeling-wip"); //SL edit, -wip as we allready have lings
        Verb changeling = new()
        {
            Text = changelingName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Weapons/Melee/armblade.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ChangelingRuleComponent>(targetPlayer, DefaultChangelingRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", changelingName, Loc.GetString("admin-verb-make-changeling-wip")), //SL edit: -wip as we have lings allready
        };
        args.Verbs.Add(changeling); */ // SpL- removed upstream lings to avoid accidents

        var paradoxCloneName = Loc.GetString("admin-verb-text-make-paradox-clone");
        Verb paradox = new()
        {
            Text = paradoxCloneName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "ParadoxClone"),
            Act = () =>
            {
                var ruleEnt = _gameTicker.AddGameRule(ParadoxCloneRuleId);

                if (!TryComp<ParadoxCloneRuleComponent>(ruleEnt, out var paradoxCloneRuleComp))
                    return;

                paradoxCloneRuleComp.OriginalBody = args.Target; // override the target player

                _gameTicker.StartGameRule(ruleEnt);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", paradoxCloneName, Loc.GetString("admin-verb-make-paradox-clone")),
        };

        var wizardName = Loc.GetString("admin-verb-text-make-wizard");
        Verb wizard = new()
        {
            Text = wizardName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "Wizard"),
            Act = () =>
            {
                // Wizard has no rule components as of writing, but I gotta put something here to satisfy the machine so just make it wizard mind rule :)
                _antag.ForceMakeAntag<WizardRoleComponent>(targetPlayer, DefaultWizardRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", wizardName, Loc.GetString("admin-verb-make-wizard")),
        };
        args.Verbs.Add(wizard);

        var ninjaName = Loc.GetString("admin-verb-text-make-space-ninja");
        Verb ninja = new()
        {
            Text = ninjaName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Weapons/Melee/energykatana.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<NinjaRoleComponent>(targetPlayer, DefaultNinjaRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", ninjaName, Loc.GetString("admin-verb-make-space-ninja")),
        };
        args.Verbs.Add(ninja);

        if (HasComp<HumanoidAppearanceComponent>(args.Target)) // only humanoids can be cloned
            args.Verbs.Add(paradox);

        var changelingName = Loc.GetString("admin-verb-text-make-changeling"); // SPL
        Verb ling = new()
        {
            Text = changelingName, // SpL
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Changeling/changeling_abilities.rsi"), "transform"),
            Act = () =>
            {
                _antag.ForceMakeAntag<SLChangelingRuleComponent>(targetPlayer, "SLChangeling");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-changeling"),
        };
        args.Verbs.Add(ling);
/// Starlight START
        var vampireName = Loc.GetString("admin-verb-text-make-vampire"); // SpL
        Verb vampire = new()
        {
            Text = vampireName, // SpL
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Starlight/Vampire/actions_vampire.rsi"), "select_class"), // Starlight
            Act = () =>
            {
                _antag.ForceMakeAntag<VampireRuleComponent>(targetPlayer, DefaultVampireRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-vampire"),
        };
        args.Verbs.Add(vampire);
		
		var selfagentName = Loc.GetString("admin-verb-text-make-selfagent");
        Verb selfagent = new()
        {
            Text = selfagentName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Starlight/Objects/Specific/SELF/freemag.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<SELFRuleComponent>(targetPlayer, DefaultSELFRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", selfagentName, Loc.GetString("admin-verb-make-selfagent")),
        };
        args.Verbs.Add(selfagent);

        if (HasComp<ShadekinComponent>(args.Target))
        {
            Verb brighteye = new()
            {
                Text = Loc.GetString("admin-verb-text-make-brighteye"),
                Category = VerbCategory.Antag,
                Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Starlight/Interface/Actions/shadekin.rsi"), "rest"),
                Act = () =>
                {
                    _gameTicker.StartGameRule("TheDarkMap"); // The Dark should always be spawned for any brighteye.
                    _antag.ForceMakeAntag<BrighteyeRuleComponent>(targetPlayer, DefaultBrighteyeRule);
                },
                Impact = LogImpact.High,
                Message = Loc.GetString("admin-verb-make-brighteye"),
            };
            args.Verbs.Add(brighteye);
        }

        var pirateSLName = Loc.GetString("admin-verb-text-make-pirate-sl");
        Verb pirateSL = new()
        {
            Text = pirateSLName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Misc/id_cards.rsi"), "pirate"),
            Act = () =>
            {
                _npcFactionSmite.RemoveFaction(args.Target, _smiteNanoTrasenFaction, false);
                _npcFactionSmite.AddFaction(args.Target, _smitePirateFaction);
                _outfit.SetOutfit(args.Target, PirateGearId); // Starlight
                EnsureComp<PirateAccentComponent>(args.Target); // Starlight

                if (_mindSystem.TryGetMind(args.Target, out var pirateMindId, out var pirateMind))
                {
                    _role.MindAddRole(pirateMindId, _pirateMindRole);
                    _mindSystem.TryAddObjective(pirateMindId, pirateMind, "PirateFollowCaptainObjective"); // Starlight
                }

                _antag.SendBriefing(args.Target,
                    Loc.GetString("pirate-crew-briefing"),
                    null,
                    new SoundPathSpecifier("/Audio/Ambience/Antag/pirate_start.ogg"));
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", pirateSLName, Loc.GetString("admin-verb-make-pirate-sl")),
        };
        args.Verbs.Add(pirateSL);
/// Starlight END
        # region SpL
        // this area handles all of our beaming behaviour, of which original content is largely not related to antags
        // beaming is a clunky but incredibly versatile tool for admins to quick-set players and themselves for admemes
        // if you wish to add a non-scheduler antag that relies on a specific or somewhat flexible loadout, copy one of the blocks below and fill as needed
        // you will need a ghost spawner, mind role, antag role, and role loadout data specified via YAML to make this work
        var centcommname = Loc.GetString("admin-verb-text-make-centcomm");
        Verb centcomm = new()
        {
            Text = centcommname,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Starlight/Interface/Misc/job_icons.rsi"), "CentComm"),
            Act = () =>
            {
                _outfit.SetOutfit(args.Target, CentCommGearID);
                _antag.ForceMakeAntag<CentCommRuleComponent>(targetPlayer, DefaultCentcommRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", centcommname, Loc.GetString("admin-verb-make-centcomm")),
        };
        args.Verbs.Add(centcomm);
            
        var cburnname = Loc.GetString("admin-verb-text-make-cburn");
        Verb cburn = new()
        {
            Text = cburnname,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Starlight/Interface/Misc/job_icons.rsi"), "CBURN"),
            Act = () =>
            {
                _outfit.SetOutfit(args.Target, CBURNGearID);
                _antag.ForceMakeAntag<CBURNRuleComponent>(targetPlayer, DefaultCBURNRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", cburnname, Loc.GetString("admin-verb-make-cburn")),
        };
        args.Verbs.Add(cburn);
        
        var cburnaltname = Loc.GetString("admin-verb-text-make-cburn-alt");
        Verb cburnalt = new()
        {
            Text = cburnaltname,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Starlight/Interface/Misc/job_icons.rsi"), "ERTChaplain"),
            Act = () =>
            {
                _outfit.SetOutfit(args.Target, CBURNGearAltID);
                _antag.ForceMakeAntag<CBURNAltRuleComponent>(targetPlayer, DefaultCBURNAltRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", cburnaltname, Loc.GetString("admin-verb-make-cburn-alt")),
        };
        args.Verbs.Add(cburnalt);
        
        var ntncname = Loc.GetString("admin-verb-text-make-ntnc-marine");
        Verb ntnc = new()
        {
            Text = ntncname,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Starlight/Interface/Misc/job_icons.rsi"), "NTNCBlueShield"),
            Act = () =>
            {
                _outfit.SetOutfit(args.Target, NTNCGearID);
                _antag.ForceMakeAntag<NTNCRuleComponent>(targetPlayer, DefaultNTNCRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", ntncname, Loc.GetString("admin-verb-make-ntnc-marine")),
        };
        args.Verbs.Add(ntnc);
        # endregion
    }
}
