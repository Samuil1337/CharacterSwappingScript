using BmSDK.Engine;

namespace Samuil1337.CharacterSwapping
{
    [Script(name: "CharacterSwappingScript")]
    sealed class CharacterSwappingScript : Script
    {
        /// <summary>
        /// Provides a read-only mapping of each playable character to its associated character information.
        /// This is useful for getting data necessary for switching characters.
        /// </summary>
        public static readonly IReadOnlyDictionary<PlayableCharacter, CharacterInfo> Characters =
            new Dictionary<PlayableCharacter, CharacterInfo>
            {
                [PlayableCharacter.BruceWayne] = new(
                    BaseId: PlayableCharacter.BruceWayne,
                    CharacterName: "Bruce_Wayne",
                    Base: "Playable_BruceWayne"
                ),
                [PlayableCharacter.Batman] = new(
                    BaseId: PlayableCharacter.Batman,
                    CharacterName: "Batman",
                    Base: "Playable_Batman"
                ),
                [PlayableCharacter.Catwoman] = new(
                    BaseId: PlayableCharacter.Catwoman,
                    CharacterName: "Catwoman",
                    Base: "Playable_Catwoman"
                ),
                [PlayableCharacter.Robin] = new(
                    BaseId: PlayableCharacter.Robin,
                    CharacterName: "Robin",
                    Base: "Playable_Robin",
                    DlcBase: "Playable_RobinStoryDLC"
                ),
                [PlayableCharacter.Nightwing] = new(
                    BaseId: PlayableCharacter.Nightwing,
                    CharacterName: "Nightwing",
                    Base: "Playable_Nightwing"
                ),
            };

        // Smoke effect on character switch
        const string SpawnEffectPkg = "Under_C2_Ch5"; // TODO: Create SF package or load together with Robin
        const string SpawnEffectPath = "FFX_Combat.Particles.NinjaSmokeBomb";
        static readonly bool s_spawnEffectEnabled = false; // TODO: Reenable spawn effect when done testing
        static readonly float s_spawnEffectScale = 1.0f;

        // Cooldown for character switch
        static readonly bool s_swapCooldownEnabled = false; // TODO: Reenable cooldown when done testing
        static readonly float s_swapCooldown = 5.0f; // The timer is scaled by seconds

        ParticleSystem? _spawnEffectTemplate;
        float _swapCooldownTimer = s_swapCooldown;

        public override void Main()
        {
            // Load in spawn effect assets if enabled
            if (s_spawnEffectEnabled)
            {
                Game.LoadPackage(SpawnEffectPkg);
                _spawnEffectTemplate = Game.FindObject<ParticleSystem>(SpawnEffectPath)!;
                _spawnEffectTemplate.AddToRoot();
            }
        }

        public override void OnLoad() => Main();

        public override void OnTick()
        {
            // Counts down timer each tick (which only occurs during gameplay)
            if (s_swapCooldownEnabled)
            {
                _swapCooldownTimer -= Game.GetDeltaTime();
            }
        }

        public override void OnKeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.F1:
                    SwapCharacter(PlayableCharacter.Batman);
                    break;
                case Keys.F2:
                    SwapCharacter(PlayableCharacter.Catwoman);
                    break;
                case Keys.F3:
                    SwapCharacter(PlayableCharacter.Robin);
                    break;
                case Keys.F4:
                    SwapCharacter(PlayableCharacter.Nightwing);
                    break;
                case Keys.F5:
                    SwapCharacter(PlayableCharacter.BruceWayne);
                    break;
            }
        }

        void SwapCharacter(PlayableCharacter character)
        {
            // Make sure swapping is allowed
            if (s_swapCooldownEnabled && _swapCooldownTimer > 0)
                return;

            var sc = new SwitchContext(
                Characters[character],
                Game.GetPlayerController(),
                _spawnEffectTemplate,
                s_spawnEffectScale
            );

            // Apply swapping cooldown
            if (sc.PerformSwitch() && s_swapCooldownEnabled)
            {
                _swapCooldownTimer = s_swapCooldown;
            }
        }
    }
}
