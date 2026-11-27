using Robust.Shared.Configuration;

namespace Content.Shared.Starlight.CCVar;

public sealed partial class StarlightCCVars
{

    /// <summary>
    /// SpL Modification: List of saved labels for the hand labeler. Labels delineated by newline, groups delineated by group separator, tabs separated by unit separator.
    /// </summary>
    public static readonly CVarDef<string> HandLabelerSavedLabels =
        CVarDef.Create("interface.hand_labeler_saved_labels", "", CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// List of saved tab names separated by unit separator.
    /// </summary>
    public static readonly CVarDef<string> HandLabelerSavedTabnames =
        CVarDef.Create("interface.hand_labeler_version", "0", CVar.CLIENTONLY | CVar.ARCHIVE);

}