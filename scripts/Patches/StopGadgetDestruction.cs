using BmSDK.BmGame;
using BmSDK.BmScript;
using BmSDK.Engine;

namespace Samuil1337.CharacterSwapping.Patches
{
    static class StopGadgetDestruction
    {
        static void BaseDestroyed(RInventoryGadget gadget) =>
            (gadget.Owner as Pawn)?.InvManager?.RemoveFromInventory(gadget);

        /// <summary>
        /// The Freeze Granade unfreezes all when it's destroyed which happens
        /// on character switch. Therefore, we need to immediately call the super function.
        /// </summary>
        [Redirect(typeof(RFreezeSpray), nameof(RFreezeSpray.Destroyed))]
        static void RFreezeSprayDestroyed(RFreezeSpray self) => BaseDestroyed(self);

        /// <summary>
        /// The Freeze Cluster Granade unfreezes all when it's destroyed which happens
        /// on character switch. Therefore, we need to immediately call the super function.
        [Redirect(typeof(RFreezeClusterGrenade), nameof(RFreezeClusterGrenade.Destroyed))]
        static void RFreezeClusterGrenadeDestroyed(RFreezeClusterGrenade self) =>
            BaseDestroyed(self);

        /// <summary>
        /// When the RPP is Destroyed() on switch, it exits the WireWalk state which then
        /// calls BMLeft(), scheduling the destruction of the Line Launcher wire. Therefore,
        /// we need to cancel this function if a switch is happening.
        /// </summary>
        [Redirect(typeof(RHidePoint_LineLauncherWire), nameof(RHidePoint_LineLauncherWire.BMLeft))]
        static void RHidePoint_LineLauncherWireBMLeft(RHidePoint_LineLauncherWire self)
        {
            if (!CharacterSwappingScript.Instance.IsInSwitch)
            {
                self.BMLeft();
            }
        }
    }
}
