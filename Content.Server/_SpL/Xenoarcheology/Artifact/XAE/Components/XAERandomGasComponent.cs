using Content.Shared.Atmos;
using Content.Shared.Atmos.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._SpL.Xenoarcheology.Artifact.XAE.Components;

/// <summary>
/// Creates a random gas on trigger
/// </summary>
[RegisterComponent, Access(typeof(XAERandomGasSystem))]
public sealed partial class XAERandomGasComponent : Component
{
    /// <summary>
    /// List of possible gasses
    /// </summary>
    [DataField] 
    //public List<Gas> PossibleGasses = new();
    public Dictionary<Gas, float> PossibleGasses = new();

    /// <summary>
    /// Moles!!! :)
    /// </summary>
    //[DataField] 
    //public float Moles = 300f;

    /// <summary>
    /// Selected Gas
    /// </summary>
    [DataField]
    //public List<Gas>? SelectedGasses;
    public Dictionary<Gas, float>? SelectedGasses;
}