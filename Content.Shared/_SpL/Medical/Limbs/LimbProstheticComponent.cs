using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Java;

namespace Content.Shared._SpL.Medical.Limbs;

[RegisterComponent]

public sealed partial class LimbProstheticComponent : Component {
    [DataField("children")]
    public bool hasChildren = true;
}