using System.Numerics;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;
using Samuil1337.CharacterSwapping.Data;

namespace Samuil1337.CharacterSwapping.State
{
    sealed class SwitchContext
    {
        static bool AreValid(params GameObject?[] objects) =>
            objects.All(obj => obj != null && obj.IsValid);

        readonly CharacterInfo _character;
        readonly RPlayerController _rpc;
        RPawnPlayer _rpp;
        readonly WorldInfo _wi;
        readonly RGameInfo _rgi;
        readonly RGameRI _gri;
        readonly RPersistentData _pData;
        readonly ParticleSystem? _effectTemplate;
        readonly float _effectScale;

        /// <summary>
        /// Initializes a switch context. To perform the actual switch
        /// use <see cref="TryPerformSwitch"/>.
        /// </summary>
        /// <param name="character">Character to switch to</param>
        /// <param name="rpc">Controller to modify</param>
        /// <param name="effectTemplate">Effect to play on switch on the RPP.
        /// May be null to disable.</param>
        /// <param name="effectScale">Scale of the effect if enabled</param>
        internal SwitchContext(
            CharacterInfo character,
            RPlayerController rpc,
            ParticleSystem? effectTemplate,
            float effectScale
        )
        {
            _character = character;
            _rpc = rpc;
            _rpp = rpc.CombatPawn;
            _wi = Game.GetWorldInfo();
            _rgi = Game.GetGameInfo();
            _gri = Game.GetGameRI();
            _pData = Game.GetPersistentData();
            _effectTemplate = effectTemplate;
            _effectScale = effectScale;
        }

        internal bool TryPerformSwitch()
        {
            // Make sure all instances are safe to use
            if (!AreValid(_rpc, _rpp, _wi, _rgi, _gri, _pData))
            {
                return false;
            }

            // Make sure switch is necessary
            if (_rpp.CharacterName == _character.CharacterName)
            {
                return false;
            }

            // Make sure switching won't break gameplay
            if (!IsSafeToSwitch())
            {
                return false;
            }

            var dto = PlayerState.FromGameState(_rpc, _pData);
            LoadAssets();
            DoSwitch();
            dto.ApplyToRpc(_rpc, _pData);

            if (_effectTemplate is not null)
            {
                PlayTransitionEffect(_rpp.Location);
            }

            return true;
        }

        bool IsSafeToSwitch()
        {
            if (_rpc.bCinematicMode || _rpc.bForceCinematicMode)
                return false;
            if (_rpc.ActiveCinematicMode is not null)
                return false;
            if (_rpc.BatmanCutscene is not null)
                return false;
            if (_rpc.IsPlayingFullScreenMovie())
                return false;
            if (_rpc.IsLookInputIgnored())
                return false;
            if (_rpc.IsMoveInputIgnored())
                return false;

            return true;
        }

        void LoadAssets()
        {
            var basePkg = _rgi.bStoryDLC ? _character.DlcBasePkg : _character.BasePkg;
            Game.LoadPackage(basePkg);

            var damageLevel = _character.SkinDamageLevel;
            var skinPkg = _character.GetSkinPkg(damageLevel);
            Game.LoadPackage(skinPkg);

            _rgi.LoadPC(_character.SkinId, damageLevel);
        }

        void DoSwitch()
        {
            // Switch character
            var act = new RSeqAct_SwitchPlayerCharacter(_wi)
            {
                CharacterName = _character.CharacterName,
                PlayerStartPoint = Game.SpawnActor<PlayerStart>(_rpp.Location, _rpp.Rotation),
            };
            _rpc.PrepareForPlayerSwitch(); // Resets HUD
            act.RestartPlayer(_rpc); // Performs switch of Pawn
            _rpp.Destroy(); // Removes old RPawnPlayer

            _rpp = _rpc.CombatPawn;
        }

        void PlayTransitionEffect(Vector3 location)
        {
            var emitter = Game.SpawnActor<Emitter>(location)!;
            emitter.SetTemplate(_effectTemplate, bDestroyOnFinish: true);
            emitter.ParticleSystemComponent.SetScale(_effectScale);
        }
    }
}
