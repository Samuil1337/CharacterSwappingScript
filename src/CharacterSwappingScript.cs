using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;
using System.Numerics;

namespace Samuil1337.CharacterSwapping;

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
                Base: "Playable_BruceWayne",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Batman] = new(
                BaseId: PlayableCharacter.Batman,
                CharacterName: "Batman",
                Base: "Playable_Batman",
                Skin: CharacterInfo.StdSkin,
                HasDamageStatePkgs: true
            ),
            [PlayableCharacter.Catwoman] = new(
                BaseId: PlayableCharacter.Catwoman,
                CharacterName: "Catwoman",
                Base: "Playable_Catwoman",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Robin] = new(
                BaseId: PlayableCharacter.Robin,
                CharacterName: "Robin",
                Base: "Playable_Robin",
                DlcBase: "Playable_RobinStoryDLC",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Nightwing] = new(
                BaseId: PlayableCharacter.Nightwing,
                CharacterName: "Nightwing",
                Base: "Playable_Nightwing",
                Skin: CharacterInfo.StdSkin
            ),
        };

    // Smoke effect on character switch
    const string SpawnEffectPkg = "Under_C2_Ch5";   // TODO: Create SF package or load together with Robin
    const string SpawnEffectPath = "FFX_Combat.Particles.NinjaSmokeBomb";
    static readonly bool SpawnEffectEnabled = false;    // TODO: Reenable spawn effect when done testing
    static readonly float SpawnEffectScale = 1.0f;
    ParticleSystem? spawnEffectTemplate;

    // Cooldown for character switch
    static readonly bool SwapCooldownEnabled = false;   // TODO: Reenable cooldown when done testing
    static readonly float SwapCooldown = 5.0f;  // The timer is scaled by seconds
    float swapCooldownTimer = SwapCooldown;

    public override void Main()
    {
        // Load in spawn effect assets if enabled
        if (SpawnEffectEnabled)
        {
            Game.LoadPackage(SpawnEffectPkg);
            spawnEffectTemplate = Game.FindObject<ParticleSystem>(SpawnEffectPath)!;
            spawnEffectTemplate.AddToRoot();
        }
    }

    public override void OnLoad() => Main();

    public override void OnTick()
    {
        // Counts down timer each tick (which only occurs during gameplay)
        if (SwapCooldownEnabled)
        {
            swapCooldownTimer -= Game.GetDeltaTime();
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
        if (SwapCooldownEnabled && swapCooldownTimer > 0) return;

        // Acquire important managers
        var wi = Game.GetWorldInfo();
        var gri = Game.GetGameRI();
        var rgi = Game.GetGameInfo();
        var pData = Game.GetPersistentData();
        var rpc = Game.GetPlayerController();
        var rpp = rpc.CombatPawn;
        if (!IsValid(wi, gri, rgi, pData, rpc, rpp)) return;

        // Make sure swapping is necessary
        var charInfo = Characters[character];
        if (rpp.CharacterName == charInfo.CharacterName) return;

        // Make sure swapping is safe
        if (!IsSafeToSwitch(rpc)) return;

        // Save data that should survive player reinstantiation
        var dto = PlayerState.FromRpc(rpc, pData);

        // Perform the actual switch
        LoadPackages(charInfo, rgi, gri);
        rpp = DoSwitch(charInfo.CharacterName, wi, rpc, rpp);

        // Fix inconsistencies after player switch
        dto.ApplyToRpc(rpc, pData);

        if (SpawnEffectEnabled)
        {
            PlayTransitionEffects(rpp.Location);
        }

        // Apply swapping cooldown
        if (SwapCooldownEnabled)
        {
            swapCooldownTimer = SwapCooldown;
        }
    }

    static bool IsValid(params GameObject?[] objects)
        => objects.All(obj => obj != null && obj.IsValid());

    static bool IsSafeToSwitch(RPlayerController rpc)
    {
        if (rpc.bCinematicMode || rpc.bForceCinematicMode) return false;
        if (rpc.ActiveCinematicMode != null) return false;
        if (rpc.BatmanCutscene != null) return false;
        if (rpc.IsPlayingFullScreenMovie()) return false;
        if (rpc.IsLookInputIgnored()) return false;
        if (rpc.IsMoveInputIgnored()) return false;

        return true;
    }

    static void LoadPackages(CharacterInfo charInfo, RGameInfo rgi, RGameRI gri)
    {
        bool isDlc = rgi.bStoryDLC;
        var basePkg = isDlc ? charInfo.DlcBasePkg : charInfo.BasePkg;
        Game.LoadPackage(basePkg);

        var damageLevel = GetDamageState(charInfo, gri);
        var skinPkg = charInfo.GetSkinPkg(damageLevel, isDlc);
        Game.LoadPackage(skinPkg);

        var skinId = isDlc ? charInfo.DlcSkinId : charInfo.SkinId;
        rgi.LoadPC(skinId, damageLevel);
    }

    static int GetDamageState(CharacterInfo charInfo, RGameRI gri)
    {
        Debug.Log(charInfo);
        // TODO: Update DamageLevel properly
        if (charInfo.BaseId is PlayableCharacter.Batman)
        {
            return 8;
        }

        return 0;
    }

    static RPawnPlayer DoSwitch(string charName, WorldInfo wi, RPlayerController rpc, RPawnPlayer rpp)
    {
        // Switch character
        var act = new RSeqAct_SwitchPlayerCharacter(wi)
        {
            CharacterName = charName,
            PlayerStartPoint = Game.SpawnActor<PlayerStart>(rpp.Location, rpp.Rotation),
        };
        rpc.PrepareForPlayerSwitch();   // Resets HUD
        act.RestartPlayer(rpc); // Performs switch of Pawn
        rpp.Destroy();  // Removes old RPawnPlayer

        return rpc.CombatPawn;
    }

    void PlayTransitionEffects(Vector3 location)
    {
        var emitter = Game.SpawnActor<Emitter>(location)!;
        emitter.SetTemplate(spawnEffectTemplate, bDestroyOnFinish: true);
        emitter.ParticleSystemComponent.SetScale(SpawnEffectScale);
    }
}
