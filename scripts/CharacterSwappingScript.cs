using BmSDK.Engine;
using Samuil1337.CharacterSwapping.Data;
using Samuil1337.CharacterSwapping.State;

namespace Samuil1337.CharacterSwapping
{
    [Script(name: "CharacterSwappingScript")]
    sealed class CharacterSwappingScript : Script
    {
        readonly SwitchConfig _switchConfig;

        ParticleSystem? _spawnEffectTemplate;
        float _swapCooldownTimer;

        public CharacterSwappingScript()
        {
            try
            {
                _switchConfig = SwitchConfig.FromToml(Mod.Config);
            }
            catch (Exception ex)
            {
                Debug.LogError("Encoutered an error parsing the config: " + ex);
                throw;
            }
        }

        public override void Main()
        {
            const string SpawnEffectPkg = "Under_C2_Ch5"; // TODO: Create SF package or load together with Robin
            const string SpawnEffectPath = "FFX_Combat.Particles.NinjaSmokeBomb";

            // Load in spawn effect assets if enabled
            if (_switchConfig.SpawnEffectEnabled)
            {
                Game.LoadPackage(SpawnEffectPkg);
                _spawnEffectTemplate = Game.FindObject<ParticleSystem>(SpawnEffectPath)!;
                _spawnEffectTemplate.AddToRoot();
            }

            // Reset timer
            if (_switchConfig.SwapCooldownEnabled)
            {
                _swapCooldownTimer = _switchConfig.SwapCooldownValue;
            }
        }

        public override void OnLoad() => Main();

        public override void OnTick()
        {
            // Counts down timer each tick (which only occurs during gameplay)
            if (_switchConfig.SwapCooldownEnabled)
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
            if (_switchConfig.SwapCooldownEnabled && _swapCooldownTimer > 0)
                return;

            var sc = new SwitchContext(
                Game.GetPlayerController(),
                character,
                _spawnEffectTemplate,
                _switchConfig.SpawnEffectScale
            );

            // Apply swapping cooldown
            if (sc.TryPerformSwitch() && _switchConfig.SwapCooldownEnabled)
            {
                _swapCooldownTimer = _switchConfig.SwapCooldownValue;
            }
        }
    }
}
