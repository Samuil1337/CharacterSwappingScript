using BmSDK;
using BmSDK.BmGame;

namespace CharacterSwapping;

/// <summary>
/// Represents an immutable Data Transfer Object of a player's state, including position, health and animations.
/// </summary>
/// <remarks>Use this record to snapshot and restore a player's state when switching characters.
/// All values are intended to be consistent with the game world at the time of capture.</remarks>
public sealed record PlayerState(
    GameObject.FVector RpcLoc, GameObject.FRotator RpcRot,
    GameObject.FVector RppLoc, GameObject.FRotator RppRot,
    int Health,
    int BallisticArmour, int MeleeArmour,
    int CWBallisticArmour, int CWMeleeArmour,
    bool DetectiveVision
)
{
    /// <summary>
    /// Creates a new DTO based on the specified RPC and persistent data.
    /// </summary>
    /// <remarks>Should be used before <see cref="DoSwitch(WorldInfo, CharacterInfo, RPawnPlayer, RPlayerController)">
    /// to capture the RPC state right before switching characters.</remarks>
    /// <param name="rpc">The RPC to snapshot before the character switch.</param>
    /// <param name="pData">The persistent data object containing player health and armor values.</param>
    /// <returns>A PlayerState snapshot of the player</returns>
    public static PlayerState FromRpc(RPlayerController rpc, RPersistentData pData)
    {
        var rpp = rpc.CombatPawn;
        return new PlayerState(
            rpc.Location, rpc.Rotation,
            rpp.Location, rpp.Rotation,
            pData.PlayerHealth,
            pData.BallisticArmour, pData.MeleeArmour,
            pData.CWBallisticArmour, pData.CWMeleeArmour,
            rpc.bInvestigateMode
        );
    }

    /// <summary>
    /// Applies the values of the DTO (before the character switch) to the RPC (after the switch),
    /// therefore, allowing for smoother transitions.
    /// </summary>
    /// <param name="rpc">The RPC to restore the state of</param>
    /// <param name="pData">The persistent data object containing player health and armor values.</param>
    public void ApplyToRpc(RPlayerController rpc, RPersistentData pData)
    {
        var rpp = rpc.CombatPawn;
        // RSeqAct_SwitchPlayerCharacter isn't reliable in Challenge Maps,
        // therefore, we must manually override the position
        rpp.SetLocation(RppLoc);
        rpp.SetRotation(RppRot);
        // Makes sure the camera doesn't change with the character swap
        rpc.SetLocation(RpcLoc);
        rpc.SetRotation(RpcRot);
        // Transfer health
        pData.PlayerHealth = Health;
        pData.BallisticArmour = BallisticArmour;
        pData.MeleeArmour = MeleeArmour;
        pData.CWBallisticArmour = CWBallisticArmour;
        pData.CWMeleeArmour = CWMeleeArmour;
        //pData.Armo
        rpp.LoadHealth();
        // Transfer Detective Mode state
        if (rpc.CurrentForensicsDevice?.bCanUseForensicsDeviceDirectly ?? false)
        {
            rpc.bInvestigateMode = DetectiveVision;
        }
        // Transfer movement state
    }
}
