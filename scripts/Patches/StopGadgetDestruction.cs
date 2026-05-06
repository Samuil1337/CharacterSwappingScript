using BmSDK.BmGame;
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
    }
}
