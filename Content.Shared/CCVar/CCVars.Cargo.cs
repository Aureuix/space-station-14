using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Whether or not the primary account of a bank should be listed
    ///     in the funding allocation console
    /// </summary>
    public static readonly CVarDef<bool> AllowPrimaryAccountAllocation =
        CVarDef.Create("cargo.allow_primary_account_allocation", false, CVar.REPLICATED);

    /// <summary>
    ///     Whether or not the primary cut of a bank should be manipulable
    ///     in the funding allocation console
    /// </summary>
    public static readonly CVarDef<bool> AllowPrimaryCutAdjustment =
        CVarDef.Create("cargo.allow_primary_cut_adjustment", true, CVar.REPLICATED);

    /// <summary>
    ///     Whether or not the separate lockbox cut is enabled
    /// </summary>
    public static readonly CVarDef<bool> LockboxCutEnabled =
        CVarDef.Create("cargo.enable_lockbox_cut", true, CVar.REPLICATED);
    
    ///SpL Start
    /// <summary>
    /// Order value multiplier for the reward given when a tamper seal is opened.
    /// </summary>
    public static readonly CVarDef<float> TamperSealRewardMultiplier =
        CVarDef.Create("cargo.tamper_seal_reward_mult", 0.1f, CVar.SERVER);

    /// <summary>
    /// Order value multiplier for the penalty applied when a tamper seal is destroyed.
    /// This is purely deducted from the deliverer.
    /// </summary>
    public static readonly CVarDef<float> TamperSealPenaltyMultiplier =
        CVarDef.Create("cargo.tamper_seal_penalty_mult", 0.1f, CVar.SERVER);

    /// <summary>
    /// Order value multiplier for the refund given to the recipient party when a tamper seal is destroyed.
    /// This is deducted from the deliverer and given to the recipient.
    /// </summary>
    public static readonly CVarDef<float> TamperSealRefundMultiplier =
        CVarDef.Create("cargo.tamper_seal_refund_mult", 0.5f, CVar.SERVER);
    ///SpL End
}
