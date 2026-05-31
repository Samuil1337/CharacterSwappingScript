using BmSDK.BmGame;
using BmSDK.Engine;
using Samuil1337.CharacterSwapping.Data;
using Samuil1337.CharacterSwapping.State;

namespace Samuil1337.CharacterSwapping
{
    [Script(name: "CharacterSwappingScript")]
    sealed class CharacterSwappingScript : Script
    {
        public static CharacterSwappingScript Instance { get; private set; } = null!;

        public SwitchConfig SwitchConfig { get; }
        public SwitchContext? CurrentSwitchContext { get; private set; }
        public bool IsInSwitch => CurrentSwitchContext is not null;

        ParticleSystem? _spawnEffectTemplate;
        float _swapCooldownTimer;

        public CharacterSwappingScript()
        {
            try
            {
                SwitchConfig = SwitchConfig.FromToml(Mod.Config);
            }
            catch (Exception ex)
            {
                Debug.LogError("Encoutered an error parsing the config: " + ex);
                throw;
            }

            Instance ??= this;
        }

        public override void Main()
        {
            // Load in spawn effect assets if enabled
            if (SwitchConfig.SpawnEffectEnabled)
            {
                _spawnEffectTemplate = Game.FindObject<ParticleSystem>(
                    "FFX_Combat.Particles.NinjaSmokeBomb"
                );
            }

            // Reset timer
            if (SwitchConfig.SwapCooldownEnabled)
            {
                _swapCooldownTimer = SwitchConfig.SwapCooldownValue;
            }
        }

        public override void OnLoad() => Main();

        public override void OnTick()
        {
            // Counts down timer each tick (which only occurs during gameplay)
            if (SwitchConfig.SwapCooldownEnabled)
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
            if (SwitchConfig.SwapCooldownEnabled && _swapCooldownTimer > 0)
                return;

            try
            {
                // Get the right in-game controller
                if (Game.GetPlayerController() is not RPlayerControllerCombat rpcc)
                {
                    return;
                }

                CurrentSwitchContext = new SwitchContext(
                    rpcc,
                    character,
                    _spawnEffectTemplate,
                    SwitchConfig.SpawnEffectScale
                );

                // Apply swapping cooldown
                if (CurrentSwitchContext.TryPerformSwitch())
                {
                    if (SwitchConfig.SwapCooldownEnabled)
                    {
                        _swapCooldownTimer = SwitchConfig.SwapCooldownValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not perform switch successfully: " + ex);
            }
            finally
            {
                CurrentSwitchContext = null;
            }
        }
    }
}
