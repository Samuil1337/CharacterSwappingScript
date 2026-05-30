using System.Numerics;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;

namespace Samuil1337.CharacterSwapping.Patches
{
    [ScriptComponent(AutoAttach = true)]
    sealed class FixJewelleryMissionComponent : ScriptComponent<RPawnVillain>
    {
        const int MaxJewellery = 16;

        [Redirect(typeof(RHudExtensionHealth), nameof(RHudExtensionHealth.Init))]
        static bool RHudExtensionHealthInit(
            RHudExtensionHealth self,
            RPlayerController rpc,
            FString extensionName,
            FString extensionPath
        )
        {
            var result = self.Init(rpc, extensionName, extensionPath);

            if (!Game.GetGameInfo().IsChallengeMode())
            {
                if (rpc.CombatPawn is not RPawnPlayerCatwomanBase)
                {
                    var pData = Game.GetPersistentData();
                    var jewellery = Math.Min(pData.NumJewelleryFound, MaxJewellery);
                    self.SetLootBar(jewellery, MaxJewellery);
                }
            }

            return result;
        }

        /// <summary>
        /// Overrides the function responsible for rendering things in Detective Mode.
        /// By default, only Catwoman can see enemies with jewellery which we change here.
        /// </summary>
        [ComponentRedirect(nameof(RPawnVillain.SetInXrayMode))]
        void SetInXrayMode(bool show, bool forceOff)
        {
            Owner.SetInXrayMode(show, forceOff);

            // Show CW jewellery indicator in other's Xray
            if (show && !Owner.ThermalSet && !Owner.bDiedAlready)
            {
                if (!Game.GetPersistentData().FinishedCatwomanJewellery())
                {
                    var jewelParticles = Owner.CatVisionParticleFX;
                    if (jewelParticles is not null)
                    {
                        jewelParticles.ActivateSystem();
                        Owner.Mesh.DetachComponent(jewelParticles);
                        Owner.Mesh.AttachComponent(jewelParticles, "Bip01_Spine1");
                    }
                }
            }
        }

        /// <summary>
        /// Overrides the function responsible for incrementing Catwoman's loot.
        /// By default, only Catwoman can collect it which we change here.
        /// </summary>
        [ComponentRedirect(nameof(RPawnVillain.Died))]
        bool Died(Controller killer, Class damageType, Vector3 hitLocation)
        {
            // Replicate Jewellery side mission logic for other characters
            if (!Owner.bDiedAlready)
            {
                var rpc = Game.GetPlayerController();
                if (rpc.CombatPawn is not RPawnPlayerCatwomanBase)
                {
                    var pData = Game.GetPersistentData();
                    if (Owner.JewelleryValue > 0 && !pData.FinishedCatwomanJewellery())
                    {
                        Owner.CatVisionParticleFX?.DeactivateSystem();
                        Owner.DeadParticleFX?.ActivateSystem();
                        if (pData.NumJewelleryFound == 0)
                        {
                            Owner.TriggerCatwomanJewellery(1);
                        }

                        var gri = Game.GetGameRI();
                        var flagMan = gri.FlagManager;
                        flagMan.SetGlobalFlag("CatJewellsCollected" + Owner.JewelleryIndex, true);

                        if (pData.IncreaseCatwomanJewellery(rpc, (int)Owner.JewelleryValue))
                        {
                            Owner.TriggerCatwomanJewellery(0);
                        }
                        else
                        {
                            gri.AutoSaveAtNearestStartPoint();
                        }

                        Owner.SetTimer(3.0f, false, "HideCatJewelBar");
                    }
                }
            }

            // Call vanilla function to handle the remaining logic + Catwoman
            return Owner.Died(killer, damageType, hitLocation);
        }
    }
}
