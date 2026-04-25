using BmSDK.BmGame;
using static Samuil1337.CharacterSwapping.Data.CharacterRegistry;

namespace Samuil1337.CharacterSwapping.Patches
{
    static class FixVisuals
    {
        // TODO: Modify RPawnVillain.SetInXrayMode() directly when redirects patched
        [Redirect(typeof(RPersistentData), nameof(RPersistentData.FinishedCatwomanJewellery))]
        static bool FinishedCatwomanJewellery(RPersistentData self)
        {
            var catwomanName = Characters[Data.PlayableCharacter.Catwoman].CharacterName;
            if (Game.GetPlayerPawn().CharacterName != catwomanName)
            {
                return true;
            }

            return self.NumJewelleryFound == 16;
        }
    }
}
