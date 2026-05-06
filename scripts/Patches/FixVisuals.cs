using BmSDK.BmGame;
using Samuil1337.CharacterSwapping.Data;

namespace Samuil1337.CharacterSwapping.Patches
{
    static class FixVisuals
    {
        // TODO: Modify RPawnVillain.SetInXrayMode() directly when redirects patched
        /// <summary>
        /// Overrides the function used to detect if the Catwoman jewllery side mission
        /// is completed. Some UI functions don't check if Catwoman is even played and,
        /// therefore, show the diamond symbol in other character's detective vision.
        /// </summary>
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
