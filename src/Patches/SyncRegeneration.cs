using BmSDK.BmGame;
using Samuil1337.CharacterSwapping.Data;
using static BmSDK.BmGame.RPawnPlayer;

namespace Samuil1337.CharacterSwapping.Patches
{
    [ScriptComponent(AutoAttach = true)]
    sealed class SyncRegenerationComponent : ScriptComponent<RPawnPlayer>
    {
        [ComponentRedirect(nameof(RPawnPlayer.RestoreArmour))]
        void RestoreArmour(int toRecover)
        {
            // Set armor in RPersistentData
            UpdatePersistentArmor();

            // Do regeneration on save data...
            var pData = Game.GetPersistentData();

            // Set Batman, Robin, Nightwing armor
            RestoreArmorOfCharacter(
                PlayableCharacter.Batman,
                toRecover,
                () => pData.MeleeArmour,
                n => pData.MeleeArmour = n,
                () => pData.BallisticArmour,
                n => pData.BallisticArmour = n
            );

            // Set Catwoman armor
            RestoreArmorOfCharacter(
                PlayableCharacter.Catwoman,
                toRecover,
                () => pData.CWMeleeArmour,
                n => pData.CWMeleeArmour = n,
                () => pData.CWBallisticArmour,
                n => pData.CWBallisticArmour = n
            );

            // Apply new health to pawn
            LoadPersistentArmor();
        }

        void UpdatePersistentArmor()
        {
            Owner.SetPersistentMeleeArmour(
                Owner.CurrentArmourLevels[(int)EArmourType.EA_ArmourMelee]
            );
            Owner.SetPersistentBallisticArmour(
                Owner.CurrentArmourLevels[(int)EArmourType.EA_ArmourBallistic]
            );
        }

        void RestoreArmorOfCharacter(
            PlayableCharacter character,
            int toRecover,
            Func<int> meleeGetter,
            Action<int> meleeSetter,
            Func<int> ballisticGetter,
            Action<int> ballisticSetter
        )
        {
            var meleeMax = GetArmorMaximum(character, EArmourType.EA_ArmourMelee);
            if (meleeMax != -1)
            {
                meleeSetter(Math.Min(meleeGetter() + toRecover, meleeMax));
            }

            var ballisticMax = GetArmorMaximum(character, EArmourType.EA_ArmourBallistic);
            if (ballisticMax != -1)
            {
                ballisticSetter(Math.Min(ballisticGetter() + toRecover, ballisticMax));
            }
        }

        int GetArmorMaximum(PlayableCharacter character, EArmourType armorType)
        {
            var armorLevel = GetUnlockedArmorLevel(character, armorType);
            if (armorLevel == -1)
            {
                return -1;
            }

            if (armorType is EArmourType.EA_ArmourMelee)
            {
                return Owner.MeleeArmourUpgrades[armorLevel];
            }
            else
            {
                return Owner.BallisticArmourUpgrades[armorLevel];
            }
        }

        int GetUnlockedArmorLevel(PlayableCharacter character, EArmourType armorType)
        {
            var armorName = armorType is EArmourType.EA_ArmourMelee ? "Melee" : "Ballistic";

            if (character is PlayableCharacter.Catwoman)
            {
                for (int i = 2; i > 0; --i)
                {
                    if (Owner.HasUpgrade($"Unlocked_Cw{armorName}Armour{i}"))
                    {
                        return i - 1;
                    }
                }
            }
            else if (character is not PlayableCharacter.BruceWayne)
            {
                for (int i = 4; i > 0; --i)
                {
                    if (Owner.HasUpgrade($"Unlocked_{armorName}Armour{i}"))
                    {
                        return i - 1;
                    }
                }
            }

            return -1;
        }

        void LoadPersistentArmor()
        {
            Owner.SetArmourCurrent(EArmourType.EA_ArmourMelee, Owner.GetPersistentMeleeArmour());
            Owner.SetArmourCurrent(
                EArmourType.EA_ArmourBallistic,
                Owner.GetPersistentBallisticArmour()
            );
        }
    }
}
