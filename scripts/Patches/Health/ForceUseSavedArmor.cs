using BmSDK.BmGame;
using BmSDK.BmScript;

namespace Samuil1337.CharacterSwapping.Patches
{
    /// <summary>
    /// Challenge Map Robin and Nightwing don't override the functions which manage saved armor.
    /// This makes them noop, breaking health transfer and regeneration logic. HQR Robin overrides
    /// the functions which manage saved armor. However, in contrast to Batman, he stores the data
    /// in ProgressCharacterStatus. To prevent inconsistencies between the characters,
    /// we override the behavior ourselves.
    /// </summary>
    static class ForceUseSavedArmor
    {
        [Redirect(typeof(RPawnPlayerRobin), "GetPersistentMeleeArmour")]
        [Redirect(typeof(RPawnPlayerRobinStoryDLC), "GetPersistentMeleeArmour")]
        [Redirect(typeof(RPawnPlayerNightwing), "GetPersistentMeleeArmour")]
        public static int GetPersistentMeleeArmour(RPawnPlayer self) =>
            Game.GetPersistentData().MeleeArmour;

        [Redirect(typeof(RPawnPlayerRobin), "GetPersistentBallisticArmour")]
        [Redirect(typeof(RPawnPlayerRobinStoryDLC), "GetPersistentBallisticArmour")]
        [Redirect(typeof(RPawnPlayerNightwing), "GetPersistentBallisticArmour")]
        public static int GetPersistentBallisticArmour(RPawnPlayer self) =>
            Game.GetPersistentData().BallisticArmour;

        [Redirect(typeof(RPawnPlayerRobin), "SetPersistentMeleeArmour")]
        [Redirect(typeof(RPawnPlayerRobinStoryDLC), "SetPersistentMeleeArmour")]
        [Redirect(typeof(RPawnPlayerNightwing), "SetPersistentMeleeArmour")]
        public static void SetPersistentMeleeArmour(RPawnPlayer self, int armor) =>
            Game.GetPersistentData().MeleeArmour = armor;

        [Redirect(typeof(RPawnPlayerRobin), "SetPersistentBallisticArmour")]
        [Redirect(typeof(RPawnPlayerRobinStoryDLC), "SetPersistentBallisticArmour")]
        [Redirect(typeof(RPawnPlayerNightwing), "SetPersistentBallisticArmour")]
        public static void SetPersistentBallisticArmour(RPawnPlayer self, int armor) =>
            Game.GetPersistentData().BallisticArmour = armor;
    }
}
