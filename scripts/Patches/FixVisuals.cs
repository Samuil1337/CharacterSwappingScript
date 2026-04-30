using BmSDK.BmGame;
using Samuil1337.CharacterSwapping.Data;

namespace Samuil1337.CharacterSwapping.Patches
{
    static class FixVisuals
    {
        // TODO: Modify RPawnVillain.SetInXrayMode() directly when redirects patched
        [Redirect(typeof(RPersistentData), nameof(RPersistentData.FinishedCatwomanJewellery))]
        static bool FinishedCatwomanJewellery(RPersistentData self)
        {
            var catwomanName = CharacterRegistry.ByEnum(PlayableCharacter.Catwoman).CharacterName;
            if (Game.GetPlayerPawn().CharacterName != catwomanName)
            {
                return true;
            }

            return self.NumJewelleryFound == 16;
        }
    }
}
