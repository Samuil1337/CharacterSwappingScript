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
        [Redirect(typeof(RPawnPlayerRobin), nameof(RPawnPlayer.GetPersistentMeleeArmour))]
        [Redirect(typeof(RPawnPlayerRobinStoryDLC), nameof(RPawnPlayer.GetPersistentMeleeArmour))]
        [Redirect(typeof(RPawnPlayerNightwing), nameof(RPawnPlayer.GetPersistentMeleeArmour))]
        static int GetPersistentMeleeArmour(RPawnPlayer self) =>
            Game.GetPersistentData().MeleeArmour;

        [Redirect(typeof(RPawnPlayerRobin), nameof(RPawnPlayer.GetPersistentBallisticArmour))]
        [Redirect(
            typeof(RPawnPlayerRobinStoryDLC),
            nameof(RPawnPlayer.GetPersistentBallisticArmour)
        )]
        [Redirect(typeof(RPawnPlayerNightwing), nameof(RPawnPlayer.GetPersistentBallisticArmour))]
        static int GetPersistentBallisticArmour(RPawnPlayer self) =>
            Game.GetPersistentData().BallisticArmour;

        [Redirect(typeof(RPawnPlayerRobin), nameof(RPawnPlayer.SetPersistentMeleeArmour))]
        [Redirect(typeof(RPawnPlayerRobinStoryDLC), nameof(RPawnPlayer.SetPersistentMeleeArmour))]
        [Redirect(typeof(RPawnPlayerNightwing), nameof(RPawnPlayer.SetPersistentMeleeArmour))]
        static void SetPersistentMeleeArmour(RPawnPlayer self, int armor) =>
            Game.GetPersistentData().MeleeArmour = armor;

        [Redirect(typeof(RPawnPlayerRobin), nameof(RPawnPlayer.SetPersistentBallisticArmour))]
        [Redirect(
            typeof(RPawnPlayerRobinStoryDLC),
            nameof(RPawnPlayer.SetPersistentBallisticArmour)
        )]
        [Redirect(typeof(RPawnPlayerNightwing), nameof(RPawnPlayer.SetPersistentBallisticArmour))]
        static void SetPersistentBallisticArmour(RPawnPlayer self, int armor) =>
            Game.GetPersistentData().BallisticArmour = armor;
    }
}
