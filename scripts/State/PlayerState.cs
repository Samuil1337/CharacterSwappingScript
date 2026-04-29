using System.Numerics;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;
using static BmSDK.BmGame.RPawnPlayer;

namespace Samuil1337.CharacterSwapping.State
{
    /// <summary>
    /// Represents an immutable Data Transfer Object of a player's state, including position, health and animations.
    /// Use this record to snapshot and restore a player's state when switching characters.
    /// </summary>
    sealed record PlayerState(
        // Camera position
        Vector3 RpcLoc,
        Rotator RpcRot,
        // Character position
        Vector3 RppLoc,
        Rotator RppRot,
        // Base health for all characters
        int Health,
        // Batman, Robin and Nightwing armour
        int MeleeArmor,
        int BallisticArmor,
        // Catwoman armour
        int CwMeleeArmor,
        int CwBallisticArmor,
        // Detective vision
        bool XRay,
        Actor[]? JammingActors
    )
    {
        /// <summary>
        /// Creates a new DTO based on the specified RPC and persistent data. Can be applied
        /// after a switch using <see cref="ApplyToGameState(RPlayerController, RPersistentData)"/>
        /// </summary>
        /// <param name="rpc">Controller to snapshot before the character switch.</param>
        /// <param name="pData">PersistentData to snapshot because some data is invalidated</param>
        /// <returns>Snapshot of the player before the switch</returns>
        public static PlayerState FromGameState(RPlayerController rpc, RPersistentData pData)
        {
            var rpp =
                rpc.CombatPawn
                ?? throw new InvalidOperationException("Controller must possess a pawn");

            // Update persistent health
            rpp.HealthUpdated();
            rpp.SetPersistentMeleeArmour(rpp.CurrentArmourLevels[(int)EArmourType.EA_ArmourMelee]);
            rpp.SetPersistentBallisticArmour(
                rpp.CurrentArmourLevels[(int)EArmourType.EA_ArmourBallistic]
            );

            return new PlayerState(
                RpcLoc: rpc.Location,
                RpcRot: rpc.Rotation,
                RppLoc: rpp.Location,
                RppRot: rpp.Rotation,
                Health: pData.PlayerHealth,
                MeleeArmor: pData.MeleeArmour,
                BallisticArmor: pData.BallisticArmour,
                CwMeleeArmor: pData.CWMeleeArmour,
                CwBallisticArmor: pData.CWBallisticArmour,
                XRay: rpc.bInvestigateMode,
                JammingActors: rpc.CurrentForensicsDevice?.JammingActors?.ToArray()
            );
        }

        /// <summary>
        /// Applies the values of the DTO (before the character switch) to the RPC (after the switch),
        /// therefore, allowing for smoother transitions.
        /// </summary>
        /// <param name="rpc">The RPC to restore the state of</param>
        /// <param name="pData">The persistent data object containing player health and armor values.</param>
        public void ApplyToGameState(RPlayerController rpc, RPersistentData pData)
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
            pData.PlayerHealth = Health;
            pData.BallisticArmour = BallisticArmor;
            pData.MeleeArmour = MeleeArmor;
            pData.CWBallisticArmour = CwBallisticArmor;
            pData.CWMeleeArmour = CwMeleeArmor;

            // Apply to pawn
            rpp.Health = Health;
            rpp.HealthUpdated();
            rpp.SetArmourCurrent(EArmourType.EA_ArmourMelee, rpp.GetPersistentMeleeArmour());
            rpp.SetArmourCurrent(
                EArmourType.EA_ArmourBallistic,
                rpp.GetPersistentBallisticArmour()
            );

            // Refresh HUD
            rpc.ShowHealthBar(rpc.HudMovieSide);
        }

        void ApplyDetectiveVision(RPlayerController rpc)
        {
            if (rpc.CurrentForensicsDevice is null)
            {
                return;
            }

            // Add Jammers
            var device = rpc.CurrentForensicsDevice;
            if (JammingActors is not null)
            {
                device.JammingActors = [.. JammingActors];
            }

            // Transfer Detective Mode state
            if (device.bCanUseForensicsDeviceDirectly)
            {
                rpc.bInvestigateMode = XRay;
            }
        }
    }
}
