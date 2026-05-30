using BmSDK.BmScript;

namespace Samuil1337.CharacterSwapping.Patches
{
    /// <summary>
    /// Challenge Map Robin doesn't override the functions which manage saved armor.
    /// This makes them noop, breaking health transfer and regeneration logic.
    /// Therefore, we override the behavior ourselves.
    /// </summary>
    [ScriptComponent(AutoAttach = true)]
    sealed class ForceRobinUseSavedArmor : ScriptComponent<RPawnPlayerRobin>
    {
        [ComponentRedirect(nameof(RPawnPlayerRobin.GetPersistentMeleeArmour))]
        int GetPersistentMeleeArmour() => Game.GetPersistentData().MeleeArmour;

        [ComponentRedirect(nameof(RPawnPlayerRobin.GetPersistentBallisticArmour))]
        int GetPersistentBallisticArmour() => Game.GetPersistentData().BallisticArmour;

        [ComponentRedirect(nameof(RPawnPlayerRobin.SetPersistentMeleeArmour))]
        void SetPersistentMeleeArmour(int armor) => Game.GetPersistentData().MeleeArmour = armor;

        [ComponentRedirect(nameof(RPawnPlayerRobin.SetPersistentBallisticArmour))]
        void SetPersistentBallisticArmour(int armor) =>
            Game.GetPersistentData().BallisticArmour = armor;
    }

    /// <summary>
    /// HQR Robin overrides the functions which manage saved armor. However, in contrast
    /// to Batman, he stores the data in ProgressCharacterStatus. To match the other characters
    /// and prevent inconsistencies we override this behavior ourselves.
    /// </summary>
    [ScriptComponent(AutoAttach = true)]
    sealed class ForceDlcRobinUseSavedArmor : ScriptComponent<RPawnPlayerRobinStoryDLC>
    {
        [ComponentRedirect(nameof(RPawnPlayerRobinStoryDLC.GetPersistentMeleeArmour))]
        int GetPersistentMeleeArmour() => Game.GetPersistentData().MeleeArmour;

        [ComponentRedirect(nameof(RPawnPlayerRobinStoryDLC.GetPersistentBallisticArmour))]
        int GetPersistentBallisticArmour() => Game.GetPersistentData().BallisticArmour;

        [ComponentRedirect(nameof(RPawnPlayerRobinStoryDLC.SetPersistentMeleeArmour))]
        void SetPersistentMeleeArmour(int armor) => Game.GetPersistentData().MeleeArmour = armor;

        [ComponentRedirect(nameof(RPawnPlayerRobinStoryDLC.SetPersistentBallisticArmour))]
        void SetPersistentBallisticArmour(int armor) =>
            Game.GetPersistentData().BallisticArmour = armor;
    }

    /// <summary>
    /// Nightwing doesn't override the functions which manage saved armor.
    /// This makes them noop, breaking health transfer and regeneration logic.
    /// Therefore, we override the behavior ourselves.
    /// </summary>
    [ScriptComponent(AutoAttach = true)]
    sealed class ForceNightwingUseSavedArmor : ScriptComponent<RPawnPlayerNightwing>
    {
        [ComponentRedirect(nameof(RPawnPlayerNightwing.GetPersistentMeleeArmour))]
        int GetPersistentMeleeArmour() => Game.GetPersistentData().MeleeArmour;

        [ComponentRedirect(nameof(RPawnPlayerNightwing.GetPersistentBallisticArmour))]
        int GetPersistentBallisticArmour() => Game.GetPersistentData().BallisticArmour;

        [ComponentRedirect(nameof(RPawnPlayerNightwing.SetPersistentMeleeArmour))]
        void SetPersistentMeleeArmour(int armor) => Game.GetPersistentData().MeleeArmour = armor;

        [ComponentRedirect(nameof(RPawnPlayerNightwing.SetPersistentBallisticArmour))]
        void SetPersistentBallisticArmour(int armor) =>
            Game.GetPersistentData().BallisticArmour = armor;
    }
}
