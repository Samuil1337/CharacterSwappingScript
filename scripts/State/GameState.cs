using BmSDK.BmGame;
using BmSDK.BmScript;

namespace Samuil1337.CharacterSwapping.State
{
    /// <summary>
    /// Contract for DTO's copying over game state between switches.
    /// </summary>
    interface IStateComponent
    {
        void CaptureState(SwitchContext ctx);
        void ApplyState(SwitchContext ctx);
    }

    /// <summary>
    /// Represents a Data Transfer Object of the entire game state, including player position,
    /// health and animations. Use this record to snapshot and restore a player's state
    /// when switching characters.
    /// </summary>
    sealed class GameState : IStateComponent
    {
        readonly IStateComponent[] _components =
        [
            // Player state
            new MovementState(),
            new HealthState(),
            new ForensicDeviceState(),
            // World state
            new ProjectilesState(),
        ];

        internal GameState(SwitchContext ctx) => CaptureState(ctx);

        public void CaptureState(SwitchContext ctx)
        {
            ctx.Rpc.GetScriptComponent<PersistentInventory>()?.CaptureState();
            foreach (var component in _components)
            {
                component.CaptureState(ctx);
            }
        }

        public void ApplyState(SwitchContext ctx)
        {
            foreach (var component in _components)
            {
                component.ApplyState(ctx);
            }
        }

        [Redirect(typeof(RPawnPlayer), nameof(RPawnPlayer.AddDefaultInventoryAfterInit))]
        [Redirect(typeof(RPawnPlayerBm), nameof(RPawnPlayerBm.AddDefaultInventoryAfterInit))]
        static void AddDefaultInventoryAfterInit(RPawnPlayer self)
        {
            self.AddDefaultInventoryAfterInit();
            self.PlayerController?.GetScriptComponent<PersistentInventory>()?.ApplyState();
        }

        /*[Redirect(typeof(RInventoryGadget), nameof(RInventoryGadget.UpdateGadgetHUDParams))]
        static void RInventoryGadgetUpdateGadgetHUDParams(RInventoryGadget self)
        {
            self.CurrHuDAmmo = -1;
            self.CurrHuDRecharge = -1;
            self.UpdateGadgetHUDParams();
        }*/
    }
}
