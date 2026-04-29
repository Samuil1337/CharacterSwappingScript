using BmSDK.Engine;
using Samuil1337.CharacterSwapping.Data;
using Samuil1337.CharacterSwapping.State;
using static Samuil1337.CharacterSwapping.Data.CharacterRegistry;

namespace Samuil1337.CharacterSwapping
{
    [Script(name: "CharacterSwappingScript")]
    sealed class CharacterSwappingScript : Script
    {
        const string SpawnEffectPkg = "Under_C2_Ch5"; // TODO: Create SF package or load together with Robin
        const string SpawnEffectPath = "FFX_Combat.Particles.NinjaSmokeBomb";

        readonly bool _spawnEffectEnabled;
        readonly float _spawnEffectScale;
        readonly bool _swapCooldownEnabled;
        readonly float _swapCooldownValue;

        ParticleSystem? _spawnEffectTemplate;
        float _swapCooldownTimer;

        public CharacterSwappingScript()
        {
            var config = (TomlTable)Mod.Config["config"];
            _spawnEffectEnabled = Convert.ToBoolean(config["spawn_effect_enabled"]);
            _spawnEffectScale = Convert.ToSingle(config["spawn_effect_scale"]);
            _swapCooldownEnabled = Convert.ToBoolean(config["swap_cooldown_enabled"]);
            _swapCooldownValue = Convert.ToSingle(config["swap_cooldown_value"]);
        }

        public override void Main()
        {
            // Load in spawn effect assets if enabled
            if (_spawnEffectEnabled)
            {
                Game.LoadPackage(SpawnEffectPkg);
                _spawnEffectTemplate = Game.FindObject<ParticleSystem>(SpawnEffectPath)!;
                _spawnEffectTemplate.AddToRoot();
            }

            // Reset timer
            if (_swapCooldownEnabled)
            {
                _swapCooldownTimer = _swapCooldownValue;
            }
        }

        public override void OnLoad() => Main();

        public override void OnTick()
        {
            // Counts down timer each tick (which only occurs during gameplay)
            if (_swapCooldownEnabled)
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
            if (_swapCooldownEnabled && _swapCooldownTimer > 0)
                return;

            var sc = new SwitchContext(
                Game.GetPlayerController(),
                Characters[character],
                _spawnEffectTemplate,
                _spawnEffectScale
            );

            // Apply swapping cooldown
            if (sc.TryPerformSwitch() && _swapCooldownEnabled)
            {
                _swapCooldownTimer = _swapCooldownValue;
            }
        }
    }
}
