using BmSDK;
using BmSDK.BmGame;
using System.Numerics;
using static Samuil1337.CharacterSwapping.CharacterSwappingScript;

namespace Samuil1337.CharacterSwapping;

/// <summary>
/// Represents an immutable Data Transfer Object of a player's state, including position, health and animations.
/// </summary>
/// <remarks>Use this record to snapshot and restore a player's state when switching characters.
/// All values are intended to be consistent with the game world at the time of capture.</remarks>
sealed record PlayerState(
    // Camera position
    Vector3 RpcLoc, Rotator RpcRot,
    // Character position
    Vector3 RppLoc, Rotator RppRot,
    // Base health for all characters
    int Health,
    // Batman, Robin and Nightwing armour
    int BArmor, int MArmor,
    // Catwoman armour
    int CwBArmor, int CwMArmor,
    // Detective vision
    bool XRay
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
            RpcLoc: rpc.Location, RpcRot: rpc.Rotation,
            RppLoc: rpp.Location, RppRot: rpp.Rotation,
            Health: pData.PlayerHealth,
            BArmor: pData.BallisticArmour, MArmor: pData.MeleeArmour,
            CwBArmor: pData.CWBallisticArmour, CwMArmor: pData.CWMeleeArmour,
            XRay: rpc.bInvestigateMode
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

        ApplyMovement(rpc, rpp);
        ApplyHealth(rpc, rpp, pData);
        ApplyDetectiveVision(rpc);
    }

    void ApplyMovement(RPlayerController rpc, RPawnPlayer rpp)
    {
        // RSeqAct_SwitchPlayerCharacter isn't reliable in Challenge Maps,
        // therefore, we must manually override the position
        rpp.SetLocation(RppLoc);
        rpp.SetRotation(RppRot);
        // Makes sure the camera doesn't change with the character swap
        rpc.SetLocation(RpcLoc);
        rpc.SetRotation(RpcRot);
    }

    void ApplyHealth(RPlayerController rpc, RPawnPlayer rpp, RPersistentData pData)
    {
        // Transfer health in save data
        pData.BallisticArmour = BArmor;
        pData.MeleeArmour = MArmor;
        pData.CWBallisticArmour = CwBArmor;
        pData.CWMeleeArmour = CwMArmor;

        // Apply health to player
        rpp.Health = Health;
        rpp.HealthUpdated();    // Optional, seems to do multiplayer updates

        // Apply armor
        bool isCatwoman = rpp.CharacterName == Characters[PlayableCharacter.Catwoman].CharacterName;
        var bArmorToApply = isCatwoman ? CwBArmor : BArmor;
        var mArmorToApply = isCatwoman ? CwMArmor : MArmor;
        rpp.SetArmourCurrent(RPawnPlayer.EArmourType.EA_ArmourBallistic, bArmorToApply);
        rpp.SetArmourCurrent(RPawnPlayer.EArmourType.EA_ArmourMelee, mArmorToApply);

        // Refresh HUD
        rpc.InstantUpdateHealth();
        rpc.ShowHealthBar(rpc.HudMovieSide);
    }

    void ApplyDetectiveVision(RPlayerController rpc)
    {
        // Transfer Detective Mode state
        if (rpc.CurrentForensicsDevice?.bCanUseForensicsDeviceDirectly ?? false)
        {
            rpc.bInvestigateMode = XRay;
        }
    }
}
