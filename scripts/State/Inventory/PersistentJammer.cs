using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.State
{
    sealed class PersistentJammer(PersistentInventory inventory)
        : PersistentGadget<RJammerGadget>(inventory)
    {
        protected override RJammerGadget? GetGadget() => Inventory.Owner?.CombatPawn?.JammerGadget;

        public override void CaptureState()
        {
            var jammer = GetGadget()!;
            MaxAmmo = jammer.MaxAmmo;
            Ammo = jammer.Ammo;
            ReplenishTime = jammer.ReplenishTime;
            CurrentReplenishTime = jammer.CurrentRechargeTime;
        }

        public override void ApplyState(bool isOverworld)
        {
            // Gadgets are instantiated with max ammo; nothing to do
            if (Ammo >= MaxAmmo)
            {
                return;
            }

            var jammer = GetGadget()!;
            jammer.Ammo = Ammo;
            if (isOverworld)
            {
                jammer.InterThrowRechargeTime = ReplenishTime;
                jammer.CurrentRechargeTime = CurrentReplenishTime;
                jammer.SetTickIsDisabled(false);
                jammer.SetTimer(CurrentReplenishTime, false, "ReplenishAmmo");
            }
            else
            {
                jammer.InterThrowRechargeTime = -1;
                jammer.CurrentRechargeTime = 0;
            }

            jammer.UpdateGadgetHUDParams();
        }
    }
}
