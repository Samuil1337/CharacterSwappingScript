using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.Patches
{
    [ScriptComponent(AutoAttach = true)]
    sealed class FixGadgetRegenerationComponent : ScriptComponent<RProjectileGadgetBase>
    {
        /// <summary>
        /// Function used internally to slowly regenerate ammo in the overworld.
        /// The game only checks the level when scheduling this function which causes bugs.
        /// </summary>
        [ComponentRedirect(nameof(RProjectileGadgetBase.ReplenishAmmo))]
        void ReplenishAmmo()
        {
            if (Game.GetGameRI().IsOverworldGameplay())
            {
                Owner.ReplenishAmmo();
            }
        }

        /// <summary>
        /// Function used when switching between interior and overworld.
        /// The game doesn't always call OnRoomChange() when exiting a building,
        /// therefore, ammo isn't always restocked, which would prevent you from
        /// using the gadget and starting the ammo reloading cycle.
        /// </summary>
        [ComponentRedirect(nameof(RProjectileGadgetBase.OnLevelChange))]
        void OnLevelChange()
        {
            if (!Game.GetGameRI().IsOverworldGameplay())
            {
                return;
            }

            if (Owner.Ammo < Owner.MaxAmmo)
            {
                const string ReplenishFunction = "ReplenishAmmo";
                if (!Owner.IsTimerActive(ReplenishFunction))
                {
                    Owner.SetTimer(Owner.ReplenishTime, inbLoop: false, ReplenishFunction);
                }
            }
        }
    }
}
