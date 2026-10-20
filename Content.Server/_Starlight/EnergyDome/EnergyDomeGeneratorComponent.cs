using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.EnergyDome;

/// <summary>
/// component, allows an entity to generate a battery-powered energy dome of a specific type.
/// </summary>
[RegisterComponent, Access(typeof(EnergyDomeSystem))] //Access add
public sealed partial class EnergyDomeGeneratorComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] //starlight
    public bool Enabled = false;

    /// <summary>
    /// How much energy will be spent from the battery per unit of damage taken by the shield.
    /// </summary>
    [DataField]
    public float DamageEnergyDraw = 10f;

    /// <summary> SpL change
    /// Whether or not the dome should add an action to the hotbar
    /// (refer to <see cref="ToggleAction"/> for modifying this action)
    /// </summary>
    [DataField]
    public bool AddAction = true; // SpL
    
    /// <summary> SpL change
    /// Whether or not the dome should add a verb to the context menu
    /// </summary>
    [DataField]
    public bool AddVerb = true; // SpL
    
    /// <summary> SpL change
    /// Whether or not the dome can be activated by clicking on the world while in hand
    /// </summary>
    [DataField]
    public bool CanUse = true; // SpL
    
    /// <summary> SpL change
    /// Whether or not the dome can be toggled by the use action in world
    /// (by use key while not held)
    /// </summary>
    [DataField]
    public bool CanActivateInWorld = true; // SpL
    
    /// <summary> SpL change
    /// Whether or not the dome can be toggled using the "use" keys
    /// (via in-hand interaction)
    /// </summary>
    [DataField]
    public bool CanUseInteractKeys = true; // SpL
    
    /// <summary>
    /// Whether or not the dome can be toggled via standard interactions
    /// (alt verbs, using in hand, etc)
    /// Disabling this will disable all above activations
    /// </summary>
    [DataField]
    public bool CanInteractUse = true;

    /// <summary>
    /// Can the NetworkDevice system activate and deactivate the barrier?
    /// </summary>
    [DataField]
    public bool CanDeviceNetworkUse = false;

    //Dome
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId DomePrototype = "EnergyDomeSmallRed";

    [DataField]
    public EntityUid? SpawnedDome;

    /// <summary>
    /// the entity on which the shield will be hung. This is either the container containing
    /// the item or the item itself. Determined when the shield is activated,
    /// it is stored in the component for changing the protected entity.
    /// </summary>
    [DataField]
    public EntityUid? DomeParentEntity;

    //Action
    [DataField]
    public EntProtoId ToggleAction = "ActionToggleDome";

    [DataField]
    public EntityUid? ToggleActionEntity;

    //Sounds
    [DataField]
    public SoundSpecifier AccessDeniedSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

    [DataField]
    public SoundSpecifier TurnOnSound = new SoundPathSpecifier("/Audio/Machines/anomaly_sync_connect.ogg");

    [DataField]
    public SoundSpecifier EnergyOutSound = new SoundPathSpecifier("/Audio/Machines/energyshield_down.ogg");

    [DataField]
    public SoundSpecifier TurnOffSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    [DataField]
    public SoundSpecifier ParrySound = new SoundPathSpecifier("/Audio/Machines/energyshield_parry.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };

    //Ports
    [DataField]
    public ProtoId<SinkPortPrototype> TogglePort = "Toggle";

    [DataField]
    public ProtoId<SinkPortPrototype> OnPort = "On";

    [DataField]
    public ProtoId<SinkPortPrototype> OffPort = "Off";
}
