using System.Numerics;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;
using Samuil1337.CharacterSwapping.Data;

namespace Samuil1337.CharacterSwapping.Patches
{
    [ScriptComponent(AutoAttach = true)]
    sealed class FixJewelleryMissionComponent : ScriptComponent<RPawnVillain>
    {
        /// <summary>
        /// Overrides the function responsible for rendering things in Detective Mode.
        /// By default, only Catwoman can see enemies with jewellery which we change here.
        /// </summary>
        [ComponentRedirect(nameof(RPawnVillain.SetInXrayMode))]
        void SetInXrayMode(bool show, bool forceOff)
        {
            Owner.ThermalSet = true;
            Owner.SetInXrayMode(show, forceOff);
            Debug.Log($"Thermal: {Owner.ThermalSet}");
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
                var charType = CharacterRegistry.ByPawn(rpc.CombatPawn)!.BaseId;
                if (charType is not PlayableCharacter.Catwoman)
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
