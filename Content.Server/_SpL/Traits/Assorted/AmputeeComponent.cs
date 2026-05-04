using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices.Java;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._SpL.Traits.Assorted;

/// <summary>
/// This component specifies which limbs are marked as removed or replaced on spawn.
/// </summary>
[RegisterComponent]

public sealed partial class AmputeeComponent : Component
{
    [DataField]
    public bool missingLeftArm = false;

    [DataField]
    public bool missingRightArm = false;

    [DataField]
    public bool missingLeftLeg = false;

    [DataField]
    public bool missingRightLeg = false;
}