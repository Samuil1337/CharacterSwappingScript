using BmSDK.BmGame;
using BmSDK.Engine;

namespace Samuil1337.CharacterSwapping.Patches
{
    static class StopGadgetDestruction
    {
        [Redirect(typeof(RFreezeSpray), nameof(RFreezeSpray.Destroyed))]
        static void RFreezeSprayDestroyed(RFreezeSpray self)
        {
            Debug.Log("Prevented Freeze Grenade destruction");
            // Original calls base which does this logic
            (self.Owner as Pawn)?.InvManager?.RemoveFromInventory(self);
        }

        [Redirect(typeof(RFreezeClusterGrenade), nameof(RFreezeClusterGrenade.Destroyed))]
        static void RFreezeClusterGrenadeDestroyed(RFreezeClusterGrenade self)
        {
            Debug.Log("Prevented Freeze Cluster Grenade destruction");
            // Original calls base which does this logic
            (self.Owner as Pawn)?.InvManager?.RemoveFromInventory(self);
        }
    }
}
