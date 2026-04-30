using System.Numerics;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;
using Samuil1337.CharacterSwapping.Data;
using Samuil1337.CharacterSwapping.State.Player;

namespace Samuil1337.CharacterSwapping.State
{
    /// <summary>
    /// Represents the action of switching the character.
    /// Created each time a character swap is requested.
    /// </summary>
    sealed class SwitchContext
    {
        static bool AreValid(params GameObject?[] objects) =>
            objects.All(obj => obj != null && obj.IsValid);

        public RPlayerController Rpc { get; }
        public RPawnPlayer Rpp { get; private set; }
        public WorldInfo Wi { get; } = Game.GetWorldInfo();
        public RGameInfo Rgi { get; } = Game.GetGameInfo();
        public RGameRI Gri { get; } = Game.GetGameRI();
        public RPersistentData PData { get; } = Game.GetPersistentData();

        readonly CharacterInfo _character;
        readonly ParticleSystem? _effectTemplate;
        readonly float _effectScale;

        /// <summary>
        /// Initializes a switch context. To perform the actual switch
        /// use <see cref="TryPerformSwitch"/>.
        /// </summary>
        /// <param name="rpc">Controller to modify</param>
        /// <param name="character">Character to switch to</param>
        /// <param name="effectTemplate">Effect to play on switch on the RPP.
        /// May be null to disable.</param>
        /// <param name="effectScale">Scale of the effect if enabled</param>
        internal SwitchContext(
            RPlayerController rpc,
            CharacterInfo character,
            ParticleSystem? effectTemplate,
            float effectScale
        )
        {
            Rpc = rpc;
            Rpp = rpc.CombatPawn;
            _character = character;
            _effectTemplate = effectTemplate;
            _effectScale = effectScale;
        }

        /// <summary>
        /// Switches the current playable character if possible.
        /// </summary>
        /// <returns>True, if the switch was successful;
        /// false, if the switch was unsuccessful</returns>
        internal bool TryPerformSwitch()
        {
            // Make sure all instances are safe to use
            if (!AreValid(Rpc, Rpp, Wi, Rgi, Gri, PData))
            {
                return false;
            }

            // Make sure switch is necessary
            if (Rpp.CharacterName == _character.CharacterName)
            {
                return false;
            }

            // Make sure switching won't break gameplay
            if (!IsSafeToSwitch())
            {
                return false;
            }

            var dto = new PlayerState(this);
            LoadAssets();
            DoSwitch();
            dto.ApplyState(this);

            if (_effectTemplate is not null)
            {
                PlayTransitionEffect(Rpp.Location);
            }

            return true;
        }

        bool IsSafeToSwitch()
        {
            if (Rpc.bCinematicMode || Rpc.bForceCinematicMode)
                return false;
            if (Rpc.ActiveCinematicMode is not null)
                return false;
            if (Rpc.BatmanCutscene is not null)
                return false;
            if (Rpc.IsPlayingFullScreenMovie())
                return false;
            if (Rpc.IsLookInputIgnored())
                return false;
            if (Rpc.IsMoveInputIgnored())
                return false;

            return true;
        }

        void LoadAssets()
        {
            // Load packages manually because LoadPC does it async causing race condition
            var basePkg = Rgi.bStoryDLC ? _character.DlcBasePkg : _character.BasePkg;
            Game.LoadPackage(basePkg);

            var damageLevel = _character.SkinDamageLevel;
            var skinPkg = _character.GetSkinPkg(damageLevel);
            Game.LoadPackage(skinPkg);

            // Set PlayableCharacters[0] to target and populate assets in struct
            Rgi.LoadPC(_character.SkinId, damageLevel);
        }

        void DoSwitch()
        {
            // Switch character
            var act = new RSeqAct_SwitchPlayerCharacter(Wi)
            {
                CharacterName = _character.CharacterName,
                PlayerStartPoint = Game.SpawnActor<PlayerStart>(Rpp.Location, Rpp.Rotation),
            };
            Rpc.PrepareForPlayerSwitch(); // Resets HUD
            act.RestartPlayer(Rpc); // Performs switch of Pawn
            Rpp.Destroy(); // Removes old RPawnPlayer

            Rpp = Rpc.CombatPawn;
        }

        void PlayTransitionEffect(Vector3 location)
        {
            var emitter = Game.SpawnActor<Emitter>(location)!;
            emitter.SetTemplate(_effectTemplate, bDestroyOnFinish: true);
            emitter.ParticleSystemComponent.SetScale(_effectScale);
        }
    }
}
