using BmSDK.BmScript;

namespace Samuil1337.CharacterSwapping.Redirects
{
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
